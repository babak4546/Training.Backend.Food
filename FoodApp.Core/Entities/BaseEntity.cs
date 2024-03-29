﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApp.Core.Entities
{
    public class BaseEntity
    {
        public virtual int  Id { get; set; }
        public DateTime CreationTime { get; set; }
        public BaseEntity()
        {
            this.CreationTime = DateTime.Now;
        }
    }
}
