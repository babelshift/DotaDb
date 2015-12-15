﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.Models
{
    public class LiveLeagueGameHeroModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarImagePath { get; set; }
        public string Url { get; set; }
    }
}