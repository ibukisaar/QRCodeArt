using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeArt {
	public abstract class QRDataEncoder {
		/// <summary>
		/// [Version, ECCLevel]
		/// </summary>
		protected static readonly (int NumberOfDataBytes, int Numeric, int Alphanumeric, int Byte, int Kanji)[,] DataCapacityTable = {
			{ // Version 1
				(   19,   41,   25,   17,   10), // ECC = L
				(   16,   34,   20,   14,    8), // ECC = M
				(   13,   27,   16,   11,    7), // ECC = Q
				(    9,   17,   10,    7,    4), // ECC = H
			},
			{ // Version 2
				(   34,   77,   47,   32,   20), // ECC = L
				(   28,   63,   38,   26,   16), // ECC = M
				(   22,   48,   29,   20,   12), // ECC = Q
				(   16,   34,   20,   14,    8), // ECC = H
			},
			{ // Version 3
				(   55,  127,   77,   53,   32), // ECC = L
				(   44,  101,   61,   42,   26), // ECC = M
				(   34,   77,   47,   32,   20), // ECC = Q
				(   26,   58,   35,   24,   15), // ECC = H
			},
			{ // Version 4
				(   80,  187,  114,   78,   48), // ECC = L
				(   64,  149,   90,   62,   38), // ECC = M
				(   48,  111,   67,   46,   28), // ECC = Q
				(   36,   82,   50,   34,   21), // ECC = H
			},
			{ // Version 5
				(  108,  255,  154,  106,   65), // ECC = L
				(   86,  202,  122,   84,   52), // ECC = M
				(   62,  144,   87,   60,   37), // ECC = Q
				(   46,  106,   64,   44,   27), // ECC = H
			},
			{ // Version 6
				(  136,  322,  195,  134,   82), // ECC = L
				(  108,  255,  154,  106,   65), // ECC = M
				(   76,  178,  108,   74,   45), // ECC = Q
				(   60,  139,   84,   58,   36), // ECC = H
			},
			{ // Version 7
				(  156,  370,  224,  154,   95), // ECC = L
				(  124,  293,  178,  122,   75), // ECC = M
				(   88,  207,  125,   86,   53), // ECC = Q
				(   66,  154,   93,   64,   39), // ECC = H
			},
			{ // Version 8
				(  194,  461,  279,  192,  118), // ECC = L
				(  154,  365,  221,  152,   93), // ECC = M
				(  110,  259,  157,  108,   66), // ECC = Q
				(   86,  202,  122,   84,   52), // ECC = H
			},
			{ // Version 9
				(  232,  552,  335,  230,  141), // ECC = L
				(  182,  432,  262,  180,  111), // ECC = M
				(  132,  312,  189,  130,   80), // ECC = Q
				(  100,  235,  143,   98,   60), // ECC = H
			},
			{ // Version 10
				(  274,  652,  395,  271,  167), // ECC = L
				(  216,  513,  311,  213,  131), // ECC = M
				(  154,  364,  221,  151,   93), // ECC = Q
				(  122,  288,  174,  119,   74), // ECC = H
			},
			{ // Version 11
				(  324,  772,  468,  321,  198), // ECC = L
				(  254,  604,  366,  251,  155), // ECC = M
				(  180,  427,  259,  177,  109), // ECC = Q
				(  140,  331,  200,  137,   85), // ECC = H
			},
			{ // Version 12
				(  370,  883,  535,  367,  226), // ECC = L
				(  290,  691,  419,  287,  177), // ECC = M
				(  206,  489,  296,  203,  125), // ECC = Q
				(  158,  374,  227,  155,   96), // ECC = H
			},
			{ // Version 13
				(  428, 1022,  619,  425,  262), // ECC = L
				(  334,  796,  483,  331,  204), // ECC = M
				(  244,  580,  352,  241,  149), // ECC = Q
				(  180,  427,  259,  177,  109), // ECC = H
			},
			{ // Version 14
				(  461, 1101,  667,  458,  282), // ECC = L
				(  365,  871,  528,  362,  223), // ECC = M
				(  261,  621,  376,  258,  159), // ECC = Q
				(  197,  468,  283,  194,  120), // ECC = H
			},
			{ // Version 15
				(  523, 1250,  758,  520,  320), // ECC = L
				(  415,  991,  600,  412,  254), // ECC = M
				(  295,  703,  426,  292,  180), // ECC = Q
				(  223,  530,  321,  220,  136), // ECC = H
			},
			{ // Version 16
				(  589, 1408,  854,  586,  361), // ECC = L
				(  453, 1082,  656,  450,  277), // ECC = M
				(  325,  775,  470,  322,  198), // ECC = Q
				(  253,  602,  365,  250,  154), // ECC = H
			},
			{ // Version 17
				(  647, 1548,  938,  644,  397), // ECC = L
				(  507, 1212,  734,  504,  310), // ECC = M
				(  367,  876,  531,  364,  224), // ECC = Q
				(  283,  674,  408,  280,  173), // ECC = H
			},
			{ // Version 18
				(  721, 1725, 1046,  718,  442), // ECC = L
				(  563, 1346,  816,  560,  345), // ECC = M
				(  397,  948,  574,  394,  243), // ECC = Q
				(  313,  746,  452,  310,  191), // ECC = H
			},
			{ // Version 19
				(  795, 1903, 1153,  792,  488), // ECC = L
				(  627, 1500,  909,  624,  384), // ECC = M
				(  445, 1063,  644,  442,  272), // ECC = Q
				(  341,  813,  493,  338,  208), // ECC = H
			},
			{ // Version 20
				(  861, 2061, 1249,  858,  528), // ECC = L
				(  669, 1600,  970,  666,  410), // ECC = M
				(  485, 1159,  702,  482,  297), // ECC = Q
				(  385,  919,  557,  382,  235), // ECC = H
			},
			{ // Version 21
				(  932, 2232, 1352,  929,  572), // ECC = L
				(  714, 1708, 1035,  711,  438), // ECC = M
				(  512, 1224,  742,  509,  314), // ECC = Q
				(  406,  969,  587,  403,  248), // ECC = H
			},
			{ // Version 22
				( 1006, 2409, 1460, 1003,  618), // ECC = L
				(  782, 1872, 1134,  779,  480), // ECC = M
				(  568, 1358,  823,  565,  348), // ECC = Q
				(  442, 1056,  640,  439,  270), // ECC = H
			},
			{ // Version 23
				( 1094, 2620, 1588, 1091,  672), // ECC = L
				(  860, 2059, 1248,  857,  528), // ECC = M
				(  614, 1468,  890,  611,  376), // ECC = Q
				(  464, 1108,  672,  461,  284), // ECC = H
			},
			{ // Version 24
				( 1174, 2812, 1704, 1171,  721), // ECC = L
				(  914, 2188, 1326,  911,  561), // ECC = M
				(  664, 1588,  963,  661,  407), // ECC = Q
				(  514, 1228,  744,  511,  315), // ECC = H
			},
			{ // Version 25
				( 1276, 3057, 1853, 1273,  784), // ECC = L
				( 1000, 2395, 1451,  997,  614), // ECC = M
				(  718, 1718, 1041,  715,  440), // ECC = Q
				(  538, 1286,  779,  535,  330), // ECC = H
			},
			{ // Version 26
				( 1370, 3283, 1990, 1367,  842), // ECC = L
				( 1062, 2544, 1542, 1059,  652), // ECC = M
				(  754, 1804, 1094,  751,  462), // ECC = Q
				(  596, 1425,  864,  593,  365), // ECC = H
			},
			{ // Version 27
				( 1468, 3517, 2132, 1465,  902), // ECC = L
				( 1128, 2701, 1637, 1125,  692), // ECC = M
				(  808, 1933, 1172,  805,  496), // ECC = Q
				(  628, 1501,  910,  625,  385), // ECC = H
			},
			{ // Version 28
				( 1531, 3669, 2223, 1528,  940), // ECC = L
				( 1193, 2857, 1732, 1190,  732), // ECC = M
				(  871, 2085, 1263,  868,  534), // ECC = Q
				(  661, 1581,  958,  658,  405), // ECC = H
			},
			{ // Version 29
				( 1631, 3909, 2369, 1628, 1002), // ECC = L
				( 1267, 3035, 1839, 1264,  778), // ECC = M
				(  911, 2181, 1322,  908,  559), // ECC = Q
				(  701, 1677, 1016,  698,  430), // ECC = H
			},
			{ // Version 30
				( 1735, 4158, 2520, 1732, 1066), // ECC = L
				( 1373, 3289, 1994, 1370,  843), // ECC = M
				(  985, 2358, 1429,  982,  604), // ECC = Q
				(  745, 1782, 1080,  742,  457), // ECC = H
			},
			{ // Version 31
				( 1843, 4417, 2677, 1840, 1132), // ECC = L
				( 1455, 3486, 2113, 1452,  894), // ECC = M
				( 1033, 2473, 1499, 1030,  634), // ECC = Q
				(  793, 1897, 1150,  790,  486), // ECC = H
			},
			{ // Version 32
				( 1955, 4686, 2840, 1952, 1201), // ECC = L
				( 1541, 3693, 2238, 1538,  947), // ECC = M
				( 1115, 2670, 1618, 1112,  684), // ECC = Q
				(  845, 2022, 1226,  842,  518), // ECC = H
			},
			{ // Version 33
				( 2071, 4965, 3009, 2068, 1273), // ECC = L
				( 1631, 3909, 2369, 1628, 1002), // ECC = M
				( 1171, 2805, 1700, 1168,  719), // ECC = Q
				(  901, 2157, 1307,  898,  553), // ECC = H
			},
			{ // Version 34
				( 2191, 5253, 3183, 2188, 1347), // ECC = L
				( 1725, 4134, 2506, 1722, 1060), // ECC = M
				( 1231, 2949, 1787, 1228,  756), // ECC = Q
				(  961, 2301, 1394,  958,  590), // ECC = H
			},
			{ // Version 35
				( 2306, 5529, 3351, 2303, 1417), // ECC = L
				( 1812, 4343, 2632, 1809, 1113), // ECC = M
				( 1286, 3081, 1867, 1283,  790), // ECC = Q
				(  986, 2361, 1431,  983,  605), // ECC = H
			},
			{ // Version 36
				( 2434, 5836, 3537, 2431, 1496), // ECC = L
				( 1914, 4588, 2780, 1911, 1176), // ECC = M
				( 1354, 3244, 1966, 1351,  832), // ECC = Q
				( 1054, 2524, 1530, 1051,  647), // ECC = H
			},
			{ // Version 37
				( 2566, 6153, 3729, 2563, 1577), // ECC = L
				( 1992, 4775, 2894, 1989, 1224), // ECC = M
				( 1426, 3417, 2071, 1423,  876), // ECC = Q
				( 1096, 2625, 1591, 1093,  673), // ECC = H
			},
			{ // Version 38
				( 2702, 6479, 3927, 2699, 1661), // ECC = L
				( 2102, 5039, 3054, 2099, 1292), // ECC = M
				( 1502, 3599, 2181, 1499,  923), // ECC = Q
				( 1142, 2735, 1658, 1139,  701), // ECC = H
			},
			{ // Version 39
				( 2812, 6743, 4087, 2809, 1729), // ECC = L
				( 2216, 5313, 3220, 2213, 1362), // ECC = M
				( 1582, 3791, 2298, 1579,  972), // ECC = Q
				( 1222, 2927, 1774, 1219,  750), // ECC = H
			},
			{ // Version 40
				( 2956, 7089, 4296, 2953, 1817), // ECC = L
				( 2334, 5596, 3391, 2331, 1435), // ECC = M
				( 1666, 3993, 2420, 1663, 1024), // ECC = Q
				( 1276, 3057, 1852, 1273,  784), // ECC = H
			},
		};

		/// <summary>
		/// [Version, ECCLevel]
		/// </summary>
		protected static readonly (int ECCPerBytes, int BlocksInGroup1, int CodewordsInGroup1, int BlocksInGroup2, int CodewordsInGroup2)[,] ECCTable = {
			{ // Version 1
				(    7,    1,   19,    0,    0), // ECC = L
				(   10,    1,   16,    0,    0), // ECC = M
				(   13,    1,   13,    0,    0), // ECC = Q
				(   17,    1,    9,    0,    0), // ECC = H
			},
			{ // Version 2
				(   10,    1,   34,    0,    0), // ECC = L
				(   16,    1,   28,    0,    0), // ECC = M
				(   22,    1,   22,    0,    0), // ECC = Q
				(   28,    1,   16,    0,    0), // ECC = H
			},
			{ // Version 3
				(   15,    1,   55,    0,    0), // ECC = L
				(   26,    1,   44,    0,    0), // ECC = M
				(   18,    2,   17,    0,    0), // ECC = Q
				(   22,    2,   13,    0,    0), // ECC = H
			},
			{ // Version 4
				(   20,    1,   80,    0,    0), // ECC = L
				(   18,    2,   32,    0,    0), // ECC = M
				(   26,    2,   24,    0,    0), // ECC = Q
				(   16,    4,    9,    0,    0), // ECC = H
			},
			{ // Version 5
				(   26,    1,  108,    0,    0), // ECC = L
				(   24,    2,   43,    0,    0), // ECC = M
				(   18,    2,   15,    2,   16), // ECC = Q
				(   22,    2,   11,    2,   12), // ECC = H
			},
			{ // Version 6
				(   18,    2,   68,    0,    0), // ECC = L
				(   16,    4,   27,    0,    0), // ECC = M
				(   24,    4,   19,    0,    0), // ECC = Q
				(   28,    4,   15,    0,    0), // ECC = H
			},
			{ // Version 7
				(   20,    2,   78,    0,    0), // ECC = L
				(   18,    4,   31,    0,    0), // ECC = M
				(   18,    2,   14,    4,   15), // ECC = Q
				(   26,    4,   13,    1,   14), // ECC = H
			},
			{ // Version 8
				(   24,    2,   97,    0,    0), // ECC = L
				(   22,    2,   38,    2,   39), // ECC = M
				(   22,    4,   18,    2,   19), // ECC = Q
				(   26,    4,   14,    2,   15), // ECC = H
			},
			{ // Version 9
				(   30,    2,  116,    0,    0), // ECC = L
				(   22,    3,   36,    2,   37), // ECC = M
				(   20,    4,   16,    4,   17), // ECC = Q
				(   24,    4,   12,    4,   13), // ECC = H
			},
			{ // Version 10
				(   18,    2,   68,    2,   69), // ECC = L
				(   26,    4,   43,    1,   44), // ECC = M
				(   24,    6,   19,    2,   20), // ECC = Q
				(   28,    6,   15,    2,   16), // ECC = H
			},
			{ // Version 11
				(   20,    4,   81,    0,    0), // ECC = L
				(   30,    1,   50,    4,   51), // ECC = M
				(   28,    4,   22,    4,   23), // ECC = Q
				(   24,    3,   12,    8,   13), // ECC = H
			},
			{ // Version 12
				(   24,    2,   92,    2,   93), // ECC = L
				(   22,    6,   36,    2,   37), // ECC = M
				(   26,    4,   20,    6,   21), // ECC = Q
				(   28,    7,   14,    4,   15), // ECC = H
			},
			{ // Version 13
				(   26,    4,  107,    0,    0), // ECC = L
				(   22,    8,   37,    1,   38), // ECC = M
				(   24,    8,   20,    4,   21), // ECC = Q
				(   22,   12,   11,    4,   12), // ECC = H
			},
			{ // Version 14
				(   30,    3,  115,    1,  116), // ECC = L
				(   24,    4,   40,    5,   41), // ECC = M
				(   20,   11,   16,    5,   17), // ECC = Q
				(   24,   11,   12,    5,   13), // ECC = H
			},
			{ // Version 15
				(   22,    5,   87,    1,   88), // ECC = L
				(   24,    5,   41,    5,   42), // ECC = M
				(   30,    5,   24,    7,   25), // ECC = Q
				(   24,   11,   12,    7,   13), // ECC = H
			},
			{ // Version 16
				(   24,    5,   98,    1,   99), // ECC = L
				(   28,    7,   45,    3,   46), // ECC = M
				(   24,   15,   19,    2,   20), // ECC = Q
				(   30,    3,   15,   13,   16), // ECC = H
			},
			{ // Version 17
				(   28,    1,  107,    5,  108), // ECC = L
				(   28,   10,   46,    1,   47), // ECC = M
				(   28,    1,   22,   15,   23), // ECC = Q
				(   28,    2,   14,   17,   15), // ECC = H
			},
			{ // Version 18
				(   30,    5,  120,    1,  121), // ECC = L
				(   26,    9,   43,    4,   44), // ECC = M
				(   28,   17,   22,    1,   23), // ECC = Q
				(   28,    2,   14,   19,   15), // ECC = H
			},
			{ // Version 19
				(   28,    3,  113,    4,  114), // ECC = L
				(   26,    3,   44,   11,   45), // ECC = M
				(   26,   17,   21,    4,   22), // ECC = Q
				(   26,    9,   13,   16,   14), // ECC = H
			},
			{ // Version 20
				(   28,    3,  107,    5,  108), // ECC = L
				(   26,    3,   41,   13,   42), // ECC = M
				(   30,   15,   24,    5,   25), // ECC = Q
				(   28,   15,   15,   10,   16), // ECC = H
			},
			{ // Version 21
				(   28,    4,  116,    4,  117), // ECC = L
				(   26,   17,   42,    0,    0), // ECC = M
				(   28,   17,   22,    6,   23), // ECC = Q
				(   30,   19,   16,    6,   17), // ECC = H
			},
			{ // Version 22
				(   28,    2,  111,    7,  112), // ECC = L
				(   28,   17,   46,    0,    0), // ECC = M
				(   30,    7,   24,   16,   25), // ECC = Q
				(   24,   34,   13,    0,    0), // ECC = H
			},
			{ // Version 23
				(   30,    4,  121,    5,  122), // ECC = L
				(   28,    4,   47,   14,   48), // ECC = M
				(   30,   11,   24,   14,   25), // ECC = Q
				(   30,   16,   15,   14,   16), // ECC = H
			},
			{ // Version 24
				(   30,    6,  117,    4,  118), // ECC = L
				(   28,    6,   45,   14,   46), // ECC = M
				(   30,   11,   24,   16,   25), // ECC = Q
				(   30,   30,   16,    2,   17), // ECC = H
			},
			{ // Version 25
				(   26,    8,  106,    4,  107), // ECC = L
				(   28,    8,   47,   13,   48), // ECC = M
				(   30,    7,   24,   22,   25), // ECC = Q
				(   30,   22,   15,   13,   16), // ECC = H
			},
			{ // Version 26
				(   28,   10,  114,    2,  115), // ECC = L
				(   28,   19,   46,    4,   47), // ECC = M
				(   28,   28,   22,    6,   23), // ECC = Q
				(   30,   33,   16,    4,   17), // ECC = H
			},
			{ // Version 27
				(   30,    8,  122,    4,  123), // ECC = L
				(   28,   22,   45,    3,   46), // ECC = M
				(   30,    8,   23,   26,   24), // ECC = Q
				(   30,   12,   15,   28,   16), // ECC = H
			},
			{ // Version 28
				(   30,    3,  117,   10,  118), // ECC = L
				(   28,    3,   45,   23,   46), // ECC = M
				(   30,    4,   24,   31,   25), // ECC = Q
				(   30,   11,   15,   31,   16), // ECC = H
			},
			{ // Version 29
				(   30,    7,  116,    7,  117), // ECC = L
				(   28,   21,   45,    7,   46), // ECC = M
				(   30,    1,   23,   37,   24), // ECC = Q
				(   30,   19,   15,   26,   16), // ECC = H
			},
			{ // Version 30
				(   30,    5,  115,   10,  116), // ECC = L
				(   28,   19,   47,   10,   48), // ECC = M
				(   30,   15,   24,   25,   25), // ECC = Q
				(   30,   23,   15,   25,   16), // ECC = H
			},
			{ // Version 31
				(   30,   13,  115,    3,  116), // ECC = L
				(   28,    2,   46,   29,   47), // ECC = M
				(   30,   42,   24,    1,   25), // ECC = Q
				(   30,   23,   15,   28,   16), // ECC = H
			},
			{ // Version 32
				(   30,   17,  115,    0,    0), // ECC = L
				(   28,   10,   46,   23,   47), // ECC = M
				(   30,   10,   24,   35,   25), // ECC = Q
				(   30,   19,   15,   35,   16), // ECC = H
			},
			{ // Version 33
				(   30,   17,  115,    1,  116), // ECC = L
				(   28,   14,   46,   21,   47), // ECC = M
				(   30,   29,   24,   19,   25), // ECC = Q
				(   30,   11,   15,   46,   16), // ECC = H
			},
			{ // Version 34
				(   30,   13,  115,    6,  116), // ECC = L
				(   28,   14,   46,   23,   47), // ECC = M
				(   30,   44,   24,    7,   25), // ECC = Q
				(   30,   59,   16,    1,   17), // ECC = H
			},
			{ // Version 35
				(   30,   12,  121,    7,  122), // ECC = L
				(   28,   12,   47,   26,   48), // ECC = M
				(   30,   39,   24,   14,   25), // ECC = Q
				(   30,   22,   15,   41,   16), // ECC = H
			},
			{ // Version 36
				(   30,    6,  121,   14,  122), // ECC = L
				(   28,    6,   47,   34,   48), // ECC = M
				(   30,   46,   24,   10,   25), // ECC = Q
				(   30,    2,   15,   64,   16), // ECC = H
			},
			{ // Version 37
				(   30,   17,  122,    4,  123), // ECC = L
				(   28,   29,   46,   14,   47), // ECC = M
				(   30,   49,   24,   10,   25), // ECC = Q
				(   30,   24,   15,   46,   16), // ECC = H
			},
			{ // Version 38
				(   30,    4,  122,   18,  123), // ECC = L
				(   28,   13,   46,   32,   47), // ECC = M
				(   30,   48,   24,   14,   25), // ECC = Q
				(   30,   42,   15,   32,   16), // ECC = H
			},
			{ // Version 39
				(   30,   20,  117,    4,  118), // ECC = L
				(   28,   40,   47,    7,   48), // ECC = M
				(   30,   43,   24,   22,   25), // ECC = Q
				(   30,   10,   15,   67,   16), // ECC = H
			},
			{ // Version 40
				(   30,   19,  118,    6,  119), // ECC = L
				(   28,   18,   47,   31,   48), // ECC = M
				(   30,   34,   24,   34,   25), // ECC = Q
				(   30,   20,   15,   61,   16), // ECC = H
			},
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int GetIndex(ECCLevel level) => (int) level ^ 1;


		protected readonly int QRVersion;
		protected readonly ECCLevel QRECCLevel;
		public readonly (int NumberOfDataBytes, int Numeric, int Alphanumeric, int Byte, int Kanji) CapacityInfo;
		public readonly (int ECCPerBytes, int BlocksInGroup1, int CodewordsInGroup1, int BlocksInGroup2, int CodewordsInGroup2) ECCInfo;

		public abstract QRDataMode DataMode { get; }
		protected abstract int BitsOfDataLength { get; }

		public QRDataEncoder(int version, ECCLevel level) {
			QRVersion = version;
			QRECCLevel = level;
			CapacityInfo = DataCapacityTable[version - 1, GetIndex(level)];
			ECCInfo = ECCTable[version - 1, GetIndex(level)];
		}

		protected abstract BitList InternalEncode(byte[] data, int start, int length);

		private BitList DataEncode(byte[] data, int start, int length) {
			var binary = InternalEncode(data, start, length);
			var needBits = CapacityInfo.NumberOfDataBytes * 8;
			var bitResult = new BitList(needBits);
			bitResult.Write(0, (int) DataMode, 4);
			bitResult.Write(4, length, BitsOfDataLength);
			bitResult.Write(4 + BitsOfDataLength, binary, 0, binary.Count);

			var validBits = 4 + BitsOfDataLength + binary.Count;
			validBits += Math.Min(4, needBits - validBits);
			var padStart = (validBits + 7) & ~7;
			while (padStart < bitResult.Count) {
				bitResult.Write(padStart, 0b11101100, 8);
				padStart += 8;
				if (padStart < bitResult.Count) {
					bitResult.Write(padStart, 0b00010001, 8);
					padStart += 8;
				}
			}
			return bitResult;
		}

		/// <summary>
		/// 计算纠错码。r(x) = f(x) * x^n mod gp(x)
		/// <para>r(x) = (ecc[0]*x^(n-1)+ecc[1]*x^(n-2)+...+ecc[n-1]*x^0)</para>
		/// <para>f(x) = (msg[0]*x^(m-1)+msg[1]*x^(m-2)+...+msg[m-1]*x^0)</para>
		/// <para>gp(x) = (x+a^0)(x+a^1)...(x+a^(n-1))</para>
		/// <para>m为消息长度，n为纠错码长度。</para>
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		private byte[] CalculateECCWords(IReadOnlyList<byte> array) {
			var eccBytes = ECCInfo.ECCPerBytes;
			var gp = new GF.XPolynom(GF.FromExponent(0), GF.FromExponent(0));
			for (int i = 1; i < eccBytes; i++) {
				gp *= new GF.XPolynom(GF.FromExponent(i), GF.FromExponent(0));
			}

			var msgCount = array.Count;
			var msg = new GF.XPolynom(msgCount);
			for (int i = 0; i < msgCount; i++) {
				msg[msgCount - 1 - i] = GF.FromPolynom(array[i]);
			}

			var ecc = msg.MulXPow(eccBytes) % gp;
			var eccByteArray = new byte[eccBytes];
			for (int i = 0; i < ecc.PolynomsCount; i++) {
				eccByteArray[eccBytes - 1 - i] = (byte) ecc[i].Polynom;
			}
			return eccByteArray;
		}

		public IReadOnlyList<(byte[] Data, byte[] Ecc)> Encode(byte[] data, int start, int length) {
			var encodedData = DataEncode(data, start, length);
			var wordsList = new List<(byte[] Data, byte[] Ecc)>();
			for (int i = 0; i < ECCInfo.BlocksInGroup1; i++) {
				var subData = new ArraySegment<byte>(encodedData.ByteArray, i * ECCInfo.CodewordsInGroup1, ECCInfo.CodewordsInGroup1);
				var eccWords = CalculateECCWords(subData);
				wordsList.Add((subData.ToArray(), eccWords));
			}
			for (int i = 0; i < ECCInfo.BlocksInGroup2; i++) {
				var subData = new ArraySegment<byte>(encodedData.ByteArray, i * ECCInfo.CodewordsInGroup2, ECCInfo.CodewordsInGroup2);
				var eccWords = CalculateECCWords(subData);
				wordsList.Add((subData.ToArray(), eccWords));
			}
			return wordsList;
		}

		public byte[] Interlock(IReadOnlyList<(byte[] Data, byte[] Ecc)> groups) {
			var result = new List<byte>(CapacityInfo.NumberOfDataBytes + ECCInfo.ECCPerBytes * (ECCInfo.BlocksInGroup1 + ECCInfo.BlocksInGroup2));
			var maxLen = Math.Max(ECCInfo.CodewordsInGroup1, ECCInfo.CodewordsInGroup2);
			for (int i = 0; i < maxLen; i++) {
				foreach (var bytes in groups) {
					if (i < bytes.Data.Length) result.Add(bytes.Data[i]);
				}
			}
			maxLen = ECCInfo.ECCPerBytes;
			for (int i = 0; i < maxLen; i++) {
				foreach (var bytes in groups) {
					result.Add(bytes.Ecc[i]);
				}
			}
			return result.ToArray();
		}



		public static QRDataMode GuessMode(byte[] data) {
			var curr = QRDataMode.Numeric;
			foreach (var b in data) {
				if (b < 0x20 || b > 'Z' || AlphanumericEncoder.AlphanumericTable[b - 0x20] < 0) {
					return QRDataMode.Byte;
				} else if (b < '0' || b > '9') {
					curr = QRDataMode.Alphanumeric;
				}
			}
			return curr;
		}

		public static int GuessVersion(byte[] data, ECCLevel level) => GuessVersion(data.Length, level, GuessMode(data));

		public static int GuessVersion(int dataLength, ECCLevel level, QRDataMode mode) {
			int version = 1;
			int eccLevelIndex = GetIndex(level);
			switch (mode) {
				case QRDataMode.Numeric:
					for (; version <= 40; version++) {
						if (dataLength <= DataCapacityTable[version - 1, eccLevelIndex].Numeric) return version;
					}
					goto Fail;
				case QRDataMode.Alphanumeric:
					for (; version <= 40; version++) {
						if (dataLength <= DataCapacityTable[version - 1, eccLevelIndex].Alphanumeric) return version;
					}
					goto Fail;
				case QRDataMode.Byte:
					for (; version <= 40; version++) {
						if (dataLength <= DataCapacityTable[version - 1, eccLevelIndex].Byte) return version;
					}
					goto Fail;
				case QRDataMode.Kanji:
					for (; version <= 40; version++) {
						if (dataLength <= DataCapacityTable[version - 1, eccLevelIndex].Kanji) return version;
					}
					goto Fail;
			}
			Fail:
			throw new NotSupportedException("数据过大");
		}

		public static QRDataEncoder CreateEncoder(QRDataMode mode, int version, ECCLevel eccLevel) {
			switch (mode) {
				case QRDataMode.Numeric: return new NumericEncoder(version, eccLevel);
				case QRDataMode.Alphanumeric: return new AlphanumericEncoder(version, eccLevel);
				case QRDataMode.Byte: return new ByteEncoder(version, eccLevel);
				default: throw new NotSupportedException($"不支持的模式：{mode}");
			}
		}
	}
}
