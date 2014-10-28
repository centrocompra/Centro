using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessLayer.Classes
{
    public class PagedResult<TEntity>
    {
        #region Properties
        public IList<TEntity> Items { get; protected set; }
        public int TotalItems { get; set; }
        #endregion

        #region Constructor
        public PagedResult()
        {
            this.Items = new List<TEntity>();
        }
        public PagedResult(IList<TEntity> items, int totalItems)
        {
            Items = items;
            TotalItems = totalItems;
        }
        #endregion

    }
}