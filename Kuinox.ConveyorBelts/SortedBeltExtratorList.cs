using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts
{
    public class SortedBeltExtratorList : ICollection<(float Distance, IBeltExtractor BeltExtractor)>
    {
        readonly List<(float Distance, IBeltExtractor Extractor)> _beltExtractors = new();

        public void Add( (float Distance, IBeltExtractor BeltExtractor) pair )
        {
            // Add while keeping the list ordered.

            if( _beltExtractors.Count > 0 && pair.Distance <= _beltExtractors[^1].Distance )
            {
                // Distance we are inserting is smaller than at least one of the extractor.

                for( int i = 0; i < _beltExtractors.Count; i++ )
                {
                    // Loop over all extractor and insert it when we meet one too big.
                    if( pair.Distance > _beltExtractors[i].Distance )
                    {
                        _beltExtractors.Insert( i, pair );
                        return;
                    }
                }
            }
            // It mean our extractor is bigger than all the list, so it go to the end.
            _beltExtractors.Add( pair );
        }

        public float DistanceFromNext( float position )
        {
            foreach( var item in _beltExtractors )
            {
                if( item.Distance > position ) return item.Distance - position;
            }
            throw new NotImplementedException();//didnt tought what error I should put here
        }

        public bool Any => _beltExtractors.Count > 0;
        public float MinDistance => _beltExtractors[0].Distance;

        public void Remove( IBeltExtractor extractor )
            => _beltExtractors.RemoveAll( s => s.Extractor == extractor );

        public void Clear() => _beltExtractors.Clear();

        public bool Contains( (float, IBeltExtractor) item ) => _beltExtractors.Contains( item );

        public void CopyTo( (float, IBeltExtractor)[] array, int arrayIndex ) => _beltExtractors.CopyTo( array, arrayIndex );

        public bool Remove( (float, IBeltExtractor) item )=> _beltExtractors.Remove( item );

        public IEnumerator<(float, IBeltExtractor)> GetEnumerator() => _beltExtractors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _beltExtractors.GetEnumerator();

        public bool IsReadOnly => false;

        public int Count => _beltExtractors.Count;
    }
}
