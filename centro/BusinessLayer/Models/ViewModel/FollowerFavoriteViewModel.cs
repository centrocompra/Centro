using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer.Models.DataModel;

namespace BusinessLayer.Models.ViewModel
{
    public class FollowerFavoriteViewModel
    {
        public List<Follower> Followers { get; set; }
        public List<Favorite> Favorites { get; set; }
    }
}
