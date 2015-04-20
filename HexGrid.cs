﻿using HexSolver.Solver;
using MSHC.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexSolver
{
	class HexGrid : IEnumerable<KeyValuePair<Vec2i, HexagonCell>>
	{
		public int MinX { get; private set; }
		public int MaxX { get; private set; }
		public int MinY { get; private set; }
		public int MaxY { get; private set; }

		private Dictionary<Vec2i, HexagonCell> map = new Dictionary<Vec2i, HexagonCell>();

		public CounterArea CounterArea { get; private set; }

		private HexHintList _HintList;

		public HexHintList HintList
		{
			get { return _HintList ?? (_HintList = GetHintList()); }
		}

		public HexGrid()
		{
			MinX = 0;
			MaxX = 1;
			MinY = 0;
			MaxY = 1;
		}

		public void SetCounterArea(CounterArea area)
		{
			CounterArea = area;
		}

		public void SetOffsetCoordinates(int x, int y, bool odd, HexagonCell value)
		{
			if (odd)
			{
				int rx = x;
				int ry = y - ((x - 1) / 2);

				Set(rx, ry, value);
			}
			else
			{
				int rx = x;
				int ry = y - (x / 2);

				Set(rx, ry, value);
			}
		}

		public HexagonCell Get(int x, int y)
		{
			var vec = new Vec2i(x, y);

			if (map.ContainsKey(vec))
				return map[vec];
			else
				return null;
		}

		public bool Remove(int x, int y)
		{
			return map.Remove(new Vec2i(x, y));
		}

		public void Set(int x, int y, HexagonCell value)
		{
			MinX = Math.Min(MinX, x);
			MaxX = Math.Max(MaxX, x + 1);
			MinY = Math.Min(MinY, y);
			MaxY = Math.Max(MaxY, y + 1);

			_HintList = null;

			map[new Vec2i(x, y)] = value;
		}

		public IEnumerator<KeyValuePair<Vec2i, HexagonCell>> GetEnumerator()
		{
			for (int x = MinX; x < MaxX; x++)
			{
				for (int y = MinY; y < MaxY; y++)
				{
					Vec2i vec = new Vec2i(x, y);

					if (map.ContainsKey(vec))
						yield return new KeyValuePair<Vec2i, HexagonCell>(vec, Get(x, y));
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerable<KeyValuePair<Vec2i, HexagonCell>> GetSurrounding(Vec2i pos)
		{
			if (Get(pos.X + 1, pos.Y) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X + 1, pos.Y), Get(pos.X + 1, pos.Y));

			if (Get(pos.X - 1, pos.Y) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X - 1, pos.Y), Get(pos.X - 1, pos.Y));

			if (Get(pos.X, pos.Y + 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X, pos.Y + 1), Get(pos.X, pos.Y + 1));

			if (Get(pos.X, pos.Y - 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X, pos.Y - 1), Get(pos.X, pos.Y - 1));

			if (Get(pos.X + 1, pos.Y - 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X + 1, pos.Y - 1), Get(pos.X + 1, pos.Y - 1));

			if (Get(pos.X - 1, pos.Y + 1) != null)
				yield return new KeyValuePair<Vec2i, HexagonCell>(new Vec2i(pos.X - 1, pos.Y + 1), Get(pos.X - 1, pos.Y + 1));
		}

		private HexHintList GetHintList()
		{
			HexHintList list = new HexHintList(this);

			foreach (var hex in this)
			{
				switch (hex.Value.Hint.Area)
				{
					case CellHintArea.NONE:
						break;
					case CellHintArea.DIRECT:
						list.Add(new HexNeighborHint(this, hex.Value));
						break;
					case CellHintArea.CIRCLE:
						list.Add(new HexAreaHint(this, hex.Value));
						break;
					case CellHintArea.COLUMN_LEFT:
					case CellHintArea.COLUMN_DOWN:
					case CellHintArea.COLUMN_RIGHT:
						list.Add(new HexRowHint(this, hex.Value));
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return list;
		}
	}
}
