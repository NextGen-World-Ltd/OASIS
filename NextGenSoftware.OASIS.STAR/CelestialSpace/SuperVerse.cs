﻿using System;
using System.Collections.Generic;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.Utilities;

namespace NextGenSoftware.OASIS.STAR.CelestialSpace
{
    public class SuperVerse : CelestialSpace, ISuperVerse
    {
        public List<IUniverse> Universes { get; set; } = new List<IUniverse>();

        public SuperVerse() : base(HolonType.SuperVerse)
        {
            Init();
        }

        public SuperVerse(IOmiverse omniverse = null) : base(HolonType.SuperVerse) 
        {
            Init(omniverse);
        }

        public SuperVerse(Guid id, IOmiverse omniverse = null, bool autoLoad = true) : base(id, HolonType.SuperVerse, autoLoad) 
        {
            Init(omniverse);
        }

        public SuperVerse(string providerKey, ProviderType providerType, IOmiverse omniverse = null, bool autoLoad = true) : base(providerKey, providerType, HolonType.Omniverse, autoLoad)
        {
            Init(omniverse);
        }

        private void Init(IOmiverse omniverse = null)
        {
            this.CreatedOASISType = new EnumValue<OASISType>(OASISType.STARCLI);

            if (this.Id == Guid.Empty)
                this.Id = Guid.NewGuid();

            if (omniverse != null)
            {
                Mapper<IOmiverse, SuperVerse>.MapParentCelestialBodyProperties(omniverse, this);
                this.ParentOmniverse = omniverse;
                this.ParentOmniverseId = omniverse.Id;
                ParentCelestialSpace = omniverse;
                ParentCelestialSpaceId = omniverse.Id;
                ParentHolon = omniverse;
                ParentHolonId = omniverse.Id;
            }

            base.AddCelestialSpaces(Universes, false);
        }
    }
}