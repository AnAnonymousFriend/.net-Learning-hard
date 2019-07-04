﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutofacDemo
{
    public class GuidAppServiceBase : IGuidAppService
    {
        private readonly Guid _item;

        public GuidAppServiceBase()
        {
            _item = Guid.NewGuid();
        }

        public Guid GuidItem()
        {
            return _item;
        }
    }
}
