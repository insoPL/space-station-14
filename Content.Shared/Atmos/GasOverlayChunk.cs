using Content.Shared.Atmos.EntitySystems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Content.Shared.Atmos.EntitySystems.SharedGasTileOverlaySystem;

namespace Content.Shared.Atmos
{
    [Serializable, NetSerializable]
    [Access(typeof(SharedGasTileOverlaySystem))]
    public sealed class GasOverlayChunk
    {
        /// <summary>
        ///     The index of this chunk
        /// </summary>
        public readonly Vector2i Index;
        public readonly Vector2i Origin;

        public SharedFireData[] TileFireData = new SharedFireData[ChunkSize * ChunkSize];
        public SharedVisibleGasData[] TileVisibleGasData = new SharedVisibleGasData[ChunkSize * ChunkSize];
        public SharedGasTemperatureData[] TileGasTemperatureData = new SharedGasTemperatureData[ChunkSize * ChunkSize];

        [NonSerialized]
        public GameTick LastUpdate;

        public GasOverlayChunk(Vector2i index)
        {
            Index = index;
            Origin = Index * ChunkSize;
        }

        public GasOverlayChunk(GasOverlayChunk data)
        {
            Index = data.Index;
            Origin = data.Origin;

            // This does not clone the opacity array. However, this chunk cloning is only used by the client,
            // which never modifies that directly. So this should be fine.
            Array.Copy(data.TileFireData, TileFireData, data.TileFireData.Length);
            Array.Copy(data.TileVisibleGasData, TileVisibleGasData, data.TileVisibleGasData.Length);
            Array.Copy(data.TileGasTemperatureData, TileGasTemperatureData, data.TileGasTemperatureData.Length);
        }

        /// <summary>
        /// Resolve a data index into <see cref="TileData"/> for the given grid index.
        /// </summary>
        public int GetDataIndex(Vector2i gridIndices)
        {
            DebugTools.Assert(InBounds(gridIndices));
            return (gridIndices.X - Origin.X) + (gridIndices.Y - Origin.Y) * ChunkSize;
        }

        private bool InBounds(Vector2i gridIndices)
        {
            return gridIndices.X >= Origin.X &&
                gridIndices.Y >= Origin.Y &&
                gridIndices.X < Origin.X + ChunkSize &&
                gridIndices.Y < Origin.Y + ChunkSize;
        }
    }

    public struct GasChunkEnumerator
    {
        private readonly SharedFireData[] _tileFireData;
        private readonly SharedVisibleGasData[] _tileVisibleGasData;
        private readonly SharedGasTemperatureData[] _tileGasTemperatureData;
        private int _index = -1;

        public int X = ChunkSize - 1;
        public int Y = -1;

        public GasChunkEnumerator(GasOverlayChunk chunk)
        {
            _tileFireData = chunk.TileFireData;
            _tileVisibleGasData = chunk.TileVisibleGasData;
            _tileGasTemperatureData = chunk.TileGasTemperatureData;
        }

        public bool MoveNext(out SharedFireData fire, out SharedVisibleGasData visibleGas, out SharedGasTemperatureData temperature)
        {
            while (++_index < _tileFireData.Length)
            {
                X += 1;
                if (X >= ChunkSize)
                {
                    X = 0;
                    Y += 1;
                }

                fire = _tileFireData[_index];
                visibleGas = _tileVisibleGasData[_index];
                temperature = _tileGasTemperatureData[_index];

                if (!fire.Equals(default) || !visibleGas.Equals(default) || !temperature.Equals(default))
                {
                    return true;
                }
            }

            fire = default;
            visibleGas = default;
            temperature = default;
            return false;
        }
    }
}
