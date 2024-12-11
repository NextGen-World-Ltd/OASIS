﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.STAR.CelestialSpace;
using NextGenSoftware.OASIS.Common;

namespace NextGenSoftware.OASIS.STAR.CelestialBodies
{
    // At the centre of each Solar System (creates Solar Systems, Planets & Moons).
    public class StarCore : CelestialBodyCore<Star>, IStarCore
    {
        public IStar Star { get; set; }

        public StarCore(IStar star) : base()
        {
            this.Star = star;
        }

        //public StarCore(IStar star, Dictionary<ProviderType, string> providerKey) : base(providerKey)
        //{
        //    this.Star = star;
        //}

        public StarCore(IStar star, string providerKey, ProviderType providerType) : base(providerKey, providerType)
        {
            this.Star = star;
        }

        public StarCore(IStar star, Guid id) : base(id)
        {
            this.Star = star;
        }

        public async Task<OASISResult<ISolarSystem>> AddSolarSystemAsync(ISolarSystem solarSystem)
        {
            OASISResult<IHolon> holonResult = await GlobalHolonData.AddHolonToCollectionAsync(Star, solarSystem, (List<IHolon>)Mapper<ISolarSystem, Holon>.Convert(Star.ParentGalaxy.SolarSystems));
            OASISResult<ISolarSystem> result = OASISResultHelper.CopyResult(holonResult, new OASISResult<ISolarSystem>());
            result.Result = (ISolarSystem)holonResult.Result;
            return result;

            //return OASISResultHelper<IHolon, ISolarSystem>.CopyResult(
            //    await AddHolonToCollectionAsync(Star, solarSystem, (List<IHolon>)Mapper<ISolarSystem, Holon>.MapBaseHolonProperties(
            //        Star.ParentGalaxy.SolarSystems)), new OASISResult<ISolarSystem>());
        }

        public OASISResult<ISolarSystem> AddSolarSystem(ISolarSystem solarSystem)
        {
            return AddSolarSystemAsync(solarSystem).Result;
        }

        public async Task<OASISResult<IPlanet>> AddPlanetAsync(IPlanet planet)
        {
            if (Star.ParentSolarSystem == null)
                Star.ParentSolarSystem = new SolarSystem(Star.ParentSolarSystemId, Star);
                //Star.ParentSolarSystem = new SolarSystem(Star.ParentSolarSystemId);


            OASISResult<IHolon> holonResult = await GlobalHolonData.AddHolonToCollectionAsync(Star, planet, (List<IHolon>)Mapper<IPlanet, Holon>.Convert(Star.ParentSolarSystem.Planets), false);
            OASISResult<IPlanet> result = OASISResultHelper.CopyResult(holonResult, new OASISResult<IPlanet>());
            result.Result = (IPlanet)holonResult.Result;

            if (result != null && result.Result != null && !result.IsError)
            {
                //OASISResult<ICelestialBody> celestialBodyResult = await planet.SaveAsync<Planet>();
                OASISResult<ICelestialBody> celestialBodyResult = await planet.SaveAsync();
                result = OASISResultHelper.CopyResult(celestialBodyResult, result);
                result.Result = (IPlanet)celestialBodyResult.Result;
            }

            return result;
        }

        public OASISResult<IPlanet> AddPlanet(IPlanet planet)
        {
            return AddPlanetAsync(planet).Result;
        }

        //TODO: Not sure if this should be in PlanetCore instead? Because technically STAR's create planets and moons from their stardust? :) And want this to be as accurate as possibile for the OASIS Simulation... ;-)
        public async Task<OASISResult<IMoon>> AddMoonAsync(IPlanet parentPlanet, IMoon moon)
        {
            OASISResult<IMoon> result = new OASISResult<IMoon>();
            //return OASISResultHelper<IHolon, IMoon>.CopyResult(
            //    await AddHolonToCollectionAsync(parentPlanet, moon, (List<IHolon>)Mapper<IMoon, Holon>.MapBaseHolonProperties(
            //        parentPlanet.Moons)), new OASISResult<IMoon>());

            List<IHolon> holons = new List<IHolon>();
            foreach (IMoon innerMoon in parentPlanet.Moons)
                holons.Add(innerMoon);

            OASISResult<IHolon> holonResult = await GlobalHolonData.AddHolonToCollectionAsync(parentPlanet, moon, holons);
            OASISResultHelper.CopyResult(holonResult, result);
            result.Result = (IMoon)holonResult.Result;
            return result;

            //return OASISResultHelper<IHolon, IMoon>.CopyResult(
            //    await AddHolonToCollectionAsync(parentPlanet, moon, (List<IHolon>) Mapper< IMoon, Holon >.MapBaseHolonProperties(
            //          parentPlanet.Moons)), new OASISResult<IMoon>());
        }

        public OASISResult<IMoon> AddMoon(IPlanet parentPlanet, IMoon moon)
        {
            return AddMoonAsync(parentPlanet, moon).Result; //TODO: Is this the best way of doing this?
        }

        public async Task<OASISResult<IEnumerable<IPlanet>>> GetAllPlanetsForSolarSystemAsync(bool refresh = true)
        {
            OASISResult<IEnumerable<IPlanet>> result = new OASISResult<IEnumerable<IPlanet>>();
            OASISResult<IEnumerable<IHolon>> holonResult = await GetHolonsAsync(Star.ParentSolarSystem.Planets, HolonType.Planet, refresh);
            OASISResultHelper.CopyResult(holonResult, result);
            result.Result = Mapper<IHolon, Planet>.MapBaseHolonProperties(holonResult.Result);
            return result;
        }

        public OASISResult<IEnumerable<IPlanet>> GetAllPlanetsForSolarSystem(bool refresh = true)
        {
            return GetAllPlanetsForSolarSystemAsync(refresh).Result;
        }

        public async Task<OASISResult<IEnumerable<IMoon>>> GetAllMoonsForSolarSystemAsync(bool refresh = true)
        {
            OASISResult<IEnumerable<IMoon>> result = new OASISResult<IEnumerable<IMoon>>();
            OASISResult<IEnumerable<IPlanet>> planetsResult = await GetAllPlanetsForSolarSystemAsync(refresh);
            OASISResultHelper.CopyResult(planetsResult, result);

            if (!planetsResult.IsError)
            {
                List<IMoon> moons = new List<IMoon>();

                foreach (IPlanet planet in planetsResult.Result)
                {
                    OASISResult<IEnumerable<IMoon>> moonsResult = await ((IPlanetCore)planet.CelestialBodyCore).GetMoonsAsync(refresh);

                    if (!moonsResult.IsError)
                        moons.AddRange(moonsResult.Result);
                }

                result.Result = moons;
            }

            return result;
        }

        public OASISResult<IEnumerable<IMoon>> GetAllMoonsForSolarSystem(bool refresh = true)
        {
            return GetAllMoonsForSolarSystemAsync(refresh).Result;
        }
    }
}