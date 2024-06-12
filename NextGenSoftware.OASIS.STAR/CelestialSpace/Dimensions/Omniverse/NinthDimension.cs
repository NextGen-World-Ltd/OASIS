﻿using System;
using System.Collections.Generic;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Core.Enums;

namespace NextGenSoftware.OASIS.STAR.CelestialSpace
{
    public class NinthDimension : OmniverseDimension, INinthDimension
    {
        public NinthDimension(IOmiverse omniverse = null) : base(omniverse)
        {
            Init(omniverse);
        }

        public NinthDimension(Guid id, IOmiverse omniverse = null) : base(id, omniverse)
        {
            Init(omniverse);
        }

        //public NinthDimension(Dictionary<ProviderType, string> providerKey, IOmiverse omniverse = null) : base(providerKey, omniverse)
        //{
        //    Init(omniverse);
        //}

        public NinthDimension(string providerKey, ProviderType providerType, IOmiverse omniverse = null) : base(providerKey, providerType, omniverse)
        {
            Init(omniverse);
        }

        private void Init(IOmiverse omniverse = null)
        {
            if (this.Id == Guid.Empty)
                this.Id = Guid.NewGuid();

            this.Name = "The Ninth Dimension";
            this.Description = "Coming Soon...";
            this.DimensionLevel = DimensionLevel.Ninth;
            this.SuperVerse.Name = $"{this.Name} SuperVerse";
        }
    }
}