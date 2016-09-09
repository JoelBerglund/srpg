﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FuckingAround {

	public class TileClickedEventArgs : EventArgs {
		public MouseEventArgs MouseEventArgs;
		public Tile Tile;
		public TileClickedEventArgs(MouseEventArgs mea, Tile tile) {
			MouseEventArgs = mea;
			Tile = tile;
		}
	}

	public partial class TileSet {
		private Tile[,] Tiles;
		public Tile this[int x, int y]{
			get { return Tiles[x, y]; }
		}

		public Tile ClickedTile;
		public int XLength { get { return Tiles.GetLength(0); } }
		public int YLength { get { return Tiles.GetLength(1); } }
		public SolidBrush SelectedBrush = new SolidBrush(Color.Red);
		public event EventHandler<TileClickedEventArgs> TileClicked;

		public TileSet(int x, int y) {
			var defaultBrush = new SolidBrush(Color.BlanchedAlmond);
			Tiles = new Tile[x, y];
			for (int ix = 0; ix < x; ix++)
				for (int iy = 0; iy < y; iy++)
					Tiles[ix, iy] = new Tile(ix, iy, this, defaultBrush);
			foreach (var tile in Tiles) tile.TraverseCost = 1;
		}

		public bool ClickTile(MouseEventArgs e) {
			ClickedTile = SelectTile(e.X, e.Y);
			if (ClickedTile != null) {
				TileClicked(this, new TileClickedEventArgs(e, ClickedTile));
				return true;
			} else return false;
		}

		public Tile SelectTile(int x, int y) {
			if (x >= 0 && x <= Tiles.GetLength(0) * Tile.Size && y >= 0 && y <= Tiles.GetLength(1) * Tile.Size)	//within tileset area
				return Tiles[x / Tile.Size, y / Tile.Size];
			else return null;
		}

		public IEnumerable<Tile> GetShit(Tile start, int mp) {
			var accumTravCost = new Dictionary<Tile, int>();	//dictionary for accumulated traversal cost
			var tils = new LinkedList<Tile>();
			accumTravCost.Add(start, 0);
			tils.AddFirst(start);

			Action<LinkedListNode<Tile>> fun = (node) => {
				foreach (var adjT in node.Value.Adjacent)
					if (accumTravCost.ContainsKey(adjT) == false) {
						int _accumTravCost = accumTravCost[node.Value] + adjT.TraverseCost;
						if (_accumTravCost <= mp) {
							accumTravCost.Add(adjT, _accumTravCost);
							var added = false;
							for (var node2 = node.Next; node2 != null; node2 = node2.Next)
								if (accumTravCost[node2.Value] >= accumTravCost[adjT]) {
									added = true;
									tils.AddBefore(node2, adjT);
									break;
								}
							if (!added) tils.AddLast(adjT);
						}
					}
			};

			fun(tils.First);	//skip 'start'
			for (var node = tils.First.Next; node != null; node = node.Next) {
				yield return node.Value;
				fun(node);
			}
		}

		public IEnumerable<Tile> GetShit(int mp) {
			return ClickedTile != null
				? GetShit(ClickedTile, mp)
				: new Tile[0];
		}

		public IEnumerable<Tile> AsEnumerable() {
			foreach (var t in Tiles) yield return t;
		}
	}
}
