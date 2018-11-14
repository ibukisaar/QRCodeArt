using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public sealed class GaussianEliminationTarget {
		private readonly int leftVectorMaxLength;
		private readonly List<byte[]> left, right;
		private readonly List<int> rightHeader;

		public IReadOnlyList<byte[]> Left => left;
		public IReadOnlyList<byte[]> Right => right;
		public IReadOnlyList<int> LinearlyIndependent => rightHeader;


		public GaussianEliminationTarget(int leftVectorMaxLength) {
			this.leftVectorMaxLength = leftVectorMaxLength;
			left = new List<byte[]>(leftVectorMaxLength);
			right = new List<byte[]>(leftVectorMaxLength);
			rightHeader = new List<int>(leftVectorMaxLength);
		}

		static int FirstOne(byte[] vector, int start = 0) {
			for (int i = start; i < vector.Length; i++) {
				if (vector[i] != 0) return i;
			}
			return -1;
		}

		static void Xor(byte[] dst, byte[] src, int start = 0) {
			if (dst.Length != src.Length) throw new ArgumentException();
			for (int i = start; i < dst.Length; i++) {
				dst[i] ^= src[i];
			}
		}

		public bool AddVector(byte[] vector)
			=> AddVectorDamage(vector.Clone() as byte[]);

		internal bool AddVectorDamage(byte[] vector) {
			if (left.Count >= leftVectorMaxLength) return false;

			int firstOne = FirstOne(vector);
			if (firstOne < 0) return false;

			int firstRow = -1;
			List<int> eliminationRecord = new List<int>();

			bool Elimination() {
				Xor(vector, right[firstRow], firstOne);
				firstOne = FirstOne(vector, firstOne + 1);
				if (firstOne < 0) return false;
				eliminationRecord.Add(firstRow);
				return true;
			}

			if (right.Count == 0) goto Success;

			if (firstOne < rightHeader[0]) {
				goto Success;
			}
			for (firstRow++; firstRow < rightHeader.Count - 1; ) {
				if (firstOne > rightHeader[firstRow] && firstOne < rightHeader[firstRow + 1]) {
					goto Success;
				} else if (firstOne == rightHeader[firstRow]) {
					if (!Elimination()) return false;
					continue;
				}
				firstRow++;
			}
			if (firstOne > rightHeader[firstRow]) {
				goto Success;
			} else if (firstOne == rightHeader[firstRow]) {
				if (!Elimination()) return false;
			}

			Success:

			firstRow++;
			rightHeader.Insert(firstRow, firstOne);
			right.Insert(firstRow, vector);
			var newLeftVector = new byte[leftVectorMaxLength];
			newLeftVector[left.Count] = 1;
			foreach (var leftRow in eliminationRecord) {
				Xor(newLeftVector, left[leftRow]);
			}
			left.Insert(firstRow, newLeftVector);
			
			for (int row = firstRow + 1; row < rightHeader.Count; row++) {
				if (vector[rightHeader[row]] != 0) {
					Xor(vector, right[row], rightHeader[row]);
					Xor(left[firstRow], left[row]);
				}
			}
			for (int row = 0; row < firstRow; row++) {
				if (right[row][firstOne] != 0) {
					Xor(right[row], vector, firstOne);
					Xor(left[row], left[firstRow]);
				}
			}
			return true;
		}

		public override string ToString() {
			var sb = new StringBuilder();
			for (int row = 0; row < left.Count; row++) {
				for (int col = 0; col < left[0].Length; col++) {
					sb.Append(left[row][col] != 0 ? '1' : '.');
				}
				sb.Append(" | ");
				for (int col = 0; col < right[0].Length; col++) {
					sb.Append(right[row][col] != 0 ? '1' : '.');
				}
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
}
