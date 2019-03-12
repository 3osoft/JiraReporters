using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraToolCheckFramework.JiraApi
{
   public class PagingJob
   {
      private readonly long _actualResults;
      private readonly long _totalResults;

      public PagingJob(long actualResults, long totalResults)
      {
         _actualResults = actualResults;
         _totalResults = totalResults;
      }

      public bool IsPagingNecessary
      {
         get { return _actualResults < _totalResults; }
      }

      public IEnumerable<long> GetPageStarts()
      {
         int numberOfPages = (int)Math.Ceiling((double)(_totalResults - _actualResults) / _actualResults);
         IEnumerable<long> results = Enumerable.Range(1, numberOfPages).Select(x => x * _actualResults);
         return results;
      }
   }
}