﻿using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using NextGenSoftware.Utilities;
using NextGenSoftware.CLI.Engine;
using NextGenSoftware.OASIS.Common;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.Events;
using NextGenSoftware.OASIS.API.Core.Holons;
using NextGenSoftware.OASIS.API.Core.Objects;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.Core.Interfaces;
using NextGenSoftware.OASIS.API.Core.Interfaces.STAR;
using NextGenSoftware.OASIS.STAR.Enums;
using NextGenSoftware.OASIS.STAR.CLI.Lib;
using NextGenSoftware.OASIS.STAR.ErrorEventArgs;
using Console = System.Console;
using System.Security.Cryptography;
using System.Threading;

namespace NextGenSoftware.OASIS.STAR.CLI
{
    class Program
    {
        private const string DEFAULT_DNA_FOLDER = "C:\\Users\\user\\source\\repos\\Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK\\NextGenSoftware.OASIS.STAR.TestHarness\\CelestialBodyDNA";
        private const string DEFAULT_GENESIS_NAMESPACE = "NextGenSoftware.OASIS.STAR.TestHarness.Genesis";
        private const string DEFAULT_GENESIS_FOLDER = "C:\\Users\\user\\source\\repos\\Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK\\NextGenSoftware.OASIS.STAR.TestHarness\\bin\\Debug\\net8.0\\Genesis";
        private const OAPPType DEFAULT_OAPP_TYPE = OAPPType.OAPPTemplate;
        private const OAPPTemplateType DEFAULT_OAPP_TEMPLATE_TYPE = OAPPTemplateType.Console;

        //private static Planet _superWorld;
        //private static Moon _jlaMoon;
        //private static Spinner _spinner = new Spinner();
        //private static string _privateKey = ""; //Set to privatekey when testing BUT remember to remove again before checking in code! Better to use avatar methods so private key is retreived from avatar and then no need to pass them in.
        private static string[] _args = null;
        private static bool _exiting = false;
        private static bool _inMainMenu = false;

        static async Task Main(string[] args)
        {
            try
            {
                _args = args;
                ShowHeader();
                CLIEngine.ShowMessage("", false);
                Console.CancelKeyPress += Console_CancelKeyPress;

                // TODO: Not sure what events should expose on Star, StarCore and HoloNETClient?
                // I feel the events should at least be on the Star object, but then they need to be on the others to bubble them up (maybe could be hidden somehow?)
                STAR.OnCelestialSpaceLoaded += STAR_OnCelestialSpaceLoaded;
                STAR.OnCelestialSpaceSaved += STAR_OnCelestialSpaceSaved;
                STAR.OnCelestialSpaceError += STAR_OnCelestialSpaceError;
                STAR.OnCelestialSpacesLoaded += STAR_OnCelestialSpacesLoaded;
                STAR.OnCelestialSpacesSaved += STAR_OnCelestialSpacesSaved;
                STAR.OnCelestialSpacesError += STAR_OnCelestialSpacesError;
                STAR.OnCelestialBodyLoaded += STAR_OnCelestialBodyLoaded;
                STAR.OnCelestialBodySaved += STAR_OnCelestialBodySaved;
                STAR.OnCelestialBodyError += STAR_OnCelestialBodyError;
                STAR.OnCelestialBodiesLoaded += STAR_OnCelestialBodiesLoaded;
                STAR.OnCelestialBodiesSaved += STAR_OnCelestialBodiesSaved;
                STAR.OnCelestialBodiesError += STAR_OnCelestialBodiesError;
                STAR.OnZomeLoaded += STAR_OnZomeLoaded;
                STAR.OnZomeSaved += STAR_OnZomeSaved;
                STAR.OnZomeError += STAR_OnZomeError;
                STAR.OnZomesLoaded += STAR_OnZomesLoaded;
                STAR.OnZomesSaved += STAR_OnZomesSaved;
                STAR.OnZomesError += STAR_OnZomesError;
                STAR.OnHolonLoaded += STAR_OnHolonLoaded;
                STAR.OnHolonSaved += STAR_OnHolonSaved;
                STAR.OnHolonError += STAR_OnHolonError;
                STAR.OnHolonsLoaded += STAR_OnHolonsLoaded;
                STAR.OnHolonsSaved += STAR_OnHolonsSaved;
                STAR.OnHolonsError += STAR_OnHolonsError;
                STAR.OnStarIgnited += STAR_OnStarIgnited;
                STAR.OnStarError += STAR_OnStarError;
                STAR.OnStarStatusChanged += STAR_OnStarStatusChanged;
                STAR.OnOASISBooted += STAR_OnOASISBooted;
                STAR.OnOASISBootError += STAR_OnOASISBootError;
                STAR.OnDefaultCeletialBodyInit += STAR_OnDefaultCeletialBodyInit;

                STAR.IsDetailedCOSMICOutputsEnabled = CLIEngine.GetConfirmation("Do you wish to enable detailed COSMIC outputs?");
                Console.WriteLine("");
                //CLIEngine.ShowMessage("");

                STAR.IsDetailedStatusUpdatesEnabled = CLIEngine.GetConfirmation("Do you wish to enable detailed STAR ODK Status outputs?");

                Console.WriteLine("");
                
               // CLIEngine.ShowMessage("Uploading...");
               // Console.WriteLine("");
               // //CLIEngine.ShowProgressBar(0);
               //// Console.WriteLine("");
               // //CLIEngine.ShowWorkingMessage("Uploading... 0%");
               // //CLIEngine.ShowWorkingMessage("Uploading...");

               // for (int i =0; i<100; i++)
               // {
               //     //CLIEngine.UpdateWorkingMessageWithPercent(i);
               //    // CLIEngine.UpdateWorkingMessage($"Uploading... {i}%");
               //     //CLIEngine.ShowProgressBar(i, true);
               //     CLIEngine.ShowProgressBar((double)i/(double)100);
               //     Thread.Sleep(1000);
               // }
                
                //await ReadyPlayerOne(); //TODO: TEMP!  Remove after testing!

                OASISResult<IOmiverse> result = STAR.IgniteStar();

                if (result.IsError)
                    CLIEngine.ShowErrorMessage(string.Concat("Error Igniting STAR. Error Message: ", result.Message));
                else
                {
                    await STARCLI.BeamInAvatar();
                    await ReadyPlayerOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                CLIEngine.ShowErrorMessage(string.Concat("An unknown error has occured. Error Details: ", ex.ToString()));
                //AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            //e.Cancel = !CLIEngine.GetConfirmation("STAR: Are you sure you wish to exit?");
            //_exiting = !e.Cancel;

             e.Cancel = true;

            //if (_inMainMenu)
            //    e.Cancel = !CLIEngine.GetConfirmation("STAR: Are you sure you wish to exit?");
            //else
            //    e.Cancel = true;

            ////Console.WriteLine("\nThe read operation has been interrupted.");
            ////Console.WriteLine($"  Key pressed: {e.SpecialKey}");
            ////Console.WriteLine($"  Cancel property: {e.Cancel}");

            //if (e.Cancel)
            //    ReadyPlayerOne();
        }

        private static void STAR_OnDefaultCeletialBodyInit(object sender, EventArgs.DefaultCelestialBodyInitEventArgs e)
        {
            if (STAR.IsDetailedCOSMICOutputsEnabled)
            {
                IHolon holon = Mapper<ICelestialBody, Holon>.MapBaseHolonProperties(e.Result.Result);
                STARCLI.ShowHolonProperties(holon);
            }
            //ShowHolonProperties((IHolon)e.Result);
        }

        private static async Task ReadyPlayerOne()
        {
            //ShowAvatarStats(); //TODO: Temp, put back in after testing! ;-)

            CLIEngine.ShowMessage("", false);
            CLIEngine.WriteAsciMessage(" READY PLAYER ONE?", Color.Green);
            //CLIEngine.ShowMessage("", false);

            //TODO: TEMP - REMOVE AFTER TESTING! :)
            //await Test(celestialBodyDNAFolder, geneisFolder);

            bool exit = false;
            do
            {
                try
                {

                    if (_exiting)
                        exit = true;

                    _inMainMenu = true;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("");
                    CLIEngine.ShowMessage("STAR: ", false, true);
                    string input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input))
                    {
                        string[] inputArgs = input.Split(" ");

                        if (inputArgs.Length > 0)
                        {
                            switch (inputArgs[0].ToLower())
                            {
                                case "ignite":
                                    {
                                        if (!STAR.IsStarIgnited)
                                            await STAR.IgniteStarAsync();
                                        else
                                            CLIEngine.ShowErrorMessage("STAR Is Already Ignited!");
                                    }
                                    break;

                                case "extinguish":
                                    {
                                        if (STAR.IsStarIgnited)
                                            await STAR.ExtinguishSuperStarAsync();
                                        else
                                            CLIEngine.ShowErrorMessage("STAR Is Not Ignited!");
                                    }
                                    break;

                                //case "beamin":
                                //    {
                                //        if (STAR.BeamedInAvatar == null)
                                //            await STARCLI.BeamInAvatar();
                                //        else
                                //            CLIEngine.ShowErrorMessage($"Avatar {STAR.BeamedInAvatar.Username} Already Beamed In. Please Beam Out First!");
                                //    }
                                //    break;

                                //case "beamout":
                                //    {
                                //        if (STAR.BeamedInAvatar != null)
                                //        {
                                //            OASISResult<IAvatar> avatarResult = await STAR.BeamedInAvatar.BeamOutAsync();

                                //            if (avatarResult != null && !avatarResult.IsError && avatarResult.Result != null)
                                //            {
                                //                STAR.BeamedInAvatar = null;
                                //                STAR.BeamedInAvatarDetail = null;
                                //                CLIEngine.ShowSuccessMessage("Avatar Successfully Beamed Out! We Hope You Enjoyed Your Time In The OASIS! Please Come Again! :)");
                                //            }
                                //            else
                                //                CLIEngine.ShowErrorMessage($"Error Beaming Out Avatar: {avatarResult.Message}");
                                //        }
                                //        else
                                //            CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                                //    }
                                //    break;

                                case "help":
                                    {
                                        if (inputArgs.Length > 1 && inputArgs[1].ToLower() == "full")
                                            ShowCommands(true);
                                        else
                                            ShowCommands(false);
                                    }
                                    break;

                                case "version":
                                    {
                                        Console.WriteLine("");
                                        CLIEngine.ShowMessage($"STAR ODK Version: {OASISBootLoader.OASISBootLoader.STARODKVersion}", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"COSMIC ORM Version: {OASISBootLoader.OASISBootLoader.COSMICVersion}", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"OASIS Runtime Version: {OASISBootLoader.OASISBootLoader.OASISVersion}", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"OASIS Provider Versions: Coming Soon...", ConsoleColor.Green, false); //TODO Implement ASAP.
                                    }
                                    break;

                                case "status":
                                    {
                                        Console.WriteLine("");
                                        CLIEngine.ShowMessage($"STAR ODK Status: {Enum.GetName(typeof(StarStatus), STAR.Status)}", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"COSMIC ORM Status: Online", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"OASIS Runtime Status: Online", ConsoleColor.Green, false);
                                        CLIEngine.ShowMessage($"OASIS Provider Status: Coming Soon...", ConsoleColor.Green, false); //TODO Implement ASAP.
                                    }
                                    break;

                                case "exit":
                                    exit = CLIEngine.GetConfirmation("STAR: Are you sure you wish to exit?");
                                    break;

                                case "light":
                                    {
                                        object oappTypeObj = null;
                                        object genesisTypeObj = null;
                                        OAPPTemplateType oappTemplateType = DEFAULT_OAPP_TEMPLATE_TYPE;
                                        OAPPType oappType = DEFAULT_OAPP_TYPE;
                                        Guid oappTemplateId = Guid.Empty;
                                        GenesisType genesisType = GenesisType.Planet;
                                        OASISResult<CoronalEjection> lightResult = null;
                                        _inMainMenu = false;

                                        if (inputArgs.Length > 1)
                                        {
                                            if (inputArgs[1].ToLower() == "wiz")
                                                await STARCLI.LightWizardAsync();
                                            else
                                            {
                                                CLIEngine.ShowWorkingMessage("Generating OAPP...");

                                                if (inputArgs.Length > 2 && Enum.TryParse(typeof(OAPPType), inputArgs[3], true, out oappTypeObj))
                                                {
                                                    oappType = (OAPPType)oappTypeObj;

                                                    if (inputArgs.Length > 3 && Enum.TryParse(typeof(OAPPTemplateType), inputArgs[4], true, out oappTypeObj))
                                                    {
                                                        oappTemplateType = (OAPPTemplateType)oappTypeObj;

                                                        if (inputArgs.Length > 4 && Guid.TryParse(inputArgs[5], out oappTemplateId))
                                                            oappTemplateId = oappTemplateId;

                                                        if (inputArgs.Length > 8)
                                                        {
                                                            if (Enum.TryParse(typeof(GenesisType), inputArgs[9], true, out genesisTypeObj))
                                                            {
                                                                genesisType = (GenesisType)genesisTypeObj;

                                                                if (inputArgs.Length > 9)
                                                                {
                                                                    Guid parentId = Guid.Empty;

                                                                    if (Guid.TryParse(inputArgs[10], out parentId))
                                                                        lightResult = await STAR.LightAsync(inputArgs[1], inputArgs[2], oappType, oappTemplateType, oappTemplateId, genesisType, inputArgs[6], inputArgs[7], inputArgs[8], parentId);
                                                                    else
                                                                        CLIEngine.ShowErrorMessage($"The ParentCelestialBodyId Passed In ({inputArgs[6]}) Is Not Valid. Please Make Sure It Is One Of The Following: {EnumHelper.GetEnumValues(typeof(GenesisType), EnumHelperListType.ItemsSeperatedByComma)}.");
                                                                }
                                                                else
                                                                    lightResult = await STAR.LightAsync(inputArgs[1], inputArgs[2], oappType, oappTemplateType, oappTemplateId, genesisType, inputArgs[6], inputArgs[7], inputArgs[8], ProviderType.Default);
                                                            }
                                                            else
                                                                CLIEngine.ShowErrorMessage($"The GenesisType Passed In ({inputArgs[7]}) Is Not Valid. Please Make Sure It Is One Of The Following: {EnumHelper.GetEnumValues(typeof(GenesisType), EnumHelperListType.ItemsSeperatedByComma)}.");
                                                        }
                                                        else
                                                            lightResult = await STAR.LightAsync(inputArgs[1], inputArgs[2], oappType, oappTemplateType, oappTemplateId, inputArgs[6], inputArgs[7], inputArgs[8]);
                                                    }
                                                    else
                                                        CLIEngine.ShowErrorMessage($"The OAPPTemplateType Passed In ({inputArgs[4]}) Is Not Valid. Please Make Sure It Is One Of The Following: {EnumHelper.GetEnumValues(typeof(OAPPType), EnumHelperListType.ItemsSeperatedByComma)}.");
                                                }
                                                else
                                                    CLIEngine.ShowErrorMessage($"The OAPPType Passed In ({inputArgs[3]}) Is Not Valid. Please Make Sure It Is One Of The Following: {EnumHelper.GetEnumValues(typeof(OAPPType), EnumHelperListType.ItemsSeperatedByComma)}.");

                                                if (lightResult != null)
                                                {
                                                    if (!lightResult.IsError && lightResult.Result != null)
                                                        CLIEngine.ShowSuccessMessage($"OAPP Successfully Generated. ({lightResult.Message})");
                                                    else
                                                        CLIEngine.ShowErrorMessage($"Error Occured: {lightResult.Message}");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("");
                                            CLIEngine.ShowMessage("LIGHT SUBCOMMAND:", ConsoleColor.Green);
                                            Console.WriteLine("");
                                            CLIEngine.ShowMessage("OAPPName               The name of the OAPP.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage($"OAPPType               The type of the OAPP, which can be any of the following: {EnumHelper.GetEnumValues(typeof(OAPPType), EnumHelperListType.ItemsSeperatedByComma)}.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage("DnaFolder              The path to the DNA Folder which will be used to generate the OAPP from.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage("GenesisFolder          The path to the Genesis Folder where the OAPP will be created.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage("GenesisNameSpace       The namespace of the OAPP to generate.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage($"GenesisType            The Genesis Type can be any of the following: {EnumHelper.GetEnumValues(typeof(GenesisType), EnumHelperListType.ItemsSeperatedByComma)}.", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage("ParentCelestialBodyId  The ID (GUID) of the Parent CelestialBody the generated OAPP will belong to. (optional)", ConsoleColor.Green, false);
                                            CLIEngine.ShowMessage("NOTE: Use 'light wiz' to start the light wizard.", ConsoleColor.Green);

                                            if (CLIEngine.GetConfirmation("Do you wish to start the wizard?"))
                                            {
                                                Console.WriteLine("");
                                                await STARCLI.LightWizardAsync();
                                            }
                                            else
                                                Console.WriteLine("");

                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                        }
                                    }
                                    break;

                                case "bang":
                                    {
                                        _inMainMenu = false;
                                        object value = CLIEngine.GetValidInputForEnum("What type of metaverse do you wish to create?", typeof(MetaverseType));

                                        if (value != null)
                                        {
                                            MetaverseType metaverseType = (MetaverseType)value;
                                        }
                                    }
                                    break;

                                case "wiz":
                                    {
                                        _inMainMenu = false;
                                        OASISResult<CoronalEjection> lightResult = null;
                                        string OAPPName = CLIEngine.GetValidInput("What is the name of the OAPP?");
                                        object value = CLIEngine.GetValidInputForEnum("What type of OAPP do you wish to create?", typeof(OAPPType));

                                        if (value != null)
                                        {
                                            OAPPType OAPPType = (OAPPType)value;

                                            value = CLIEngine.GetValidInputForEnum("What type of GenesisType do you wish to create?", typeof(GenesisType));

                                            if (value != null)
                                            {
                                                GenesisType genesisType = (GenesisType)value;

                                                string genesisNamespace = CLIEngine.GetValidInput("What is the Genesis Namespace?");
                                                Guid parentId = Guid.Empty;

                                                if (!CLIEngine.GetConfirmation("Do you wish to add support for all OASIS Providers (recommeneded) or only specefic ones?"))
                                                {
                                                    bool providersSelected = false;
                                                    List<ProviderType> providers = new List<ProviderType>();

                                                    while (!providersSelected)
                                                    {
                                                        object providerType = CLIEngine.GetValidInputForEnum("What provider do you wish to add?", typeof(ProviderType));
                                                        providers.Add((ProviderType)providerType);

                                                        if (!CLIEngine.GetConfirmation("Do you wish to add any other providers?"))
                                                            providersSelected = true;
                                                    }
                                                }

                                                string zomeName = CLIEngine.GetValidInput("What is the name of the Zome (collection of Holons)?");
                                                string holonName = CLIEngine.GetValidInput("What is the name of the Holon (OASIS Data Object)?");
                                                string propName = CLIEngine.GetValidInput("What is the name of the Field/Property?");
                                                object propType = CLIEngine.GetValidInputForEnum("What is the type of the Field/Property?", typeof(HolonPropType));

                                                //TODO: Come back to this... :)

                                                if (CLIEngine.GetConfirmation("Does this OAPP belong to another CelestialBody?"))
                                                    parentId = CLIEngine.GetValidInputForGuid("What is the Id (GUID) of the parent CelestialBody?");


                                                if (lightResult != null)
                                                {
                                                    if (!lightResult.IsError && lightResult.Result != null)
                                                        CLIEngine.ShowSuccessMessage($"OAPP Successfully Generated. ({lightResult.Message})");
                                                    else
                                                        CLIEngine.ShowErrorMessage($"Error Occured: {lightResult.Message}");
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case "flare":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "shine":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "dim":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "seed":
                                    await STARCLI.PublishOAPPAsync();
                                    break;

                                case "unseed":
                                    await STARCLI.UnPublishOAPPAsync();
                                    break;

                                case "twinkle":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "dust":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "radiate":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "emit":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "reflect":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "evolve":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "mutate":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "love":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "burst":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "super":
                                    {
                                        CLIEngine.ShowMessage("Coming soon...");
                                    }
                                    break;

                                case "net":
                                    await STARCLI.LaunchSTARNETAsync();
                                    break;

                                case "oapp":
                                    {
                                        if (inputArgs.Length > 1)
                                        {
                                            switch (inputArgs[1].ToLower())
                                            {
                                                case "publish":
                                                    {
                                                        string oappPath = "";
                                                        bool dotNetPublish = false;

                                                        if (inputArgs.Length > 2)
                                                            oappPath = inputArgs[2];

                                                        if (inputArgs.Length > 3 && inputArgs[3].ToLower() == "dotnetpublish")
                                                            dotNetPublish = true;

                                                        await STARCLI.PublishOAPPAsync(oappPath, dotNetPublish);
                                                    }
                                                    break;

                                                case "template":
                                                    {
                                                        //await ShowOAPPTemplateSubCommandsAsync(inputArgs);
                                                        await ShowSubCommandAsync(inputArgs, "OAPP TEMPLATE", "", STARCLI.CreateOAPPTemplateAsync, STARCLI.EditOAPPAsync, STARCLI.DeleteOAPPAsync, STARCLI.InstallOAPPAsync, STARCLI.UnInstallOAPPAsync, null, STARCLI.UnPublishOAPPAsync, STARCLI.ShowOAPPAsync, STARCLI.ListOAPPsCreatedByBeamedInAvatarAsync, STARCLI.ListAllOAPPsAsync, STARCLI.ListOAPPsInstalledForBeamedInAvatarAsync, STARCLI.SearchOAPPsAsync, ProviderType.Default, true);
                                                    }
                                                    break;
                                            }
                                        }

                                        await ShowSubCommandAsync(inputArgs, "OAPP", "", STARCLI.LightWizardAsync, STARCLI.EditOAPPAsync, STARCLI.DeleteOAPPAsync, STARCLI.InstallOAPPAsync, STARCLI.UnInstallOAPPAsync, null, STARCLI.UnPublishOAPPAsync, STARCLI.ShowOAPPAsync, STARCLI.ListOAPPsCreatedByBeamedInAvatarAsync, STARCLI.ListAllOAPPsAsync, STARCLI.ListOAPPsInstalledForBeamedInAvatarAsync, STARCLI.SearchOAPPsAsync, ProviderType.Default, true);
                                        break;
                                    }

                                case "happ":
                                    {
                                        if (inputArgs.Length > 1)
                                        {
                                            switch (inputArgs[1].ToLower())
                                            {
                                                case "publish":
                                                    {
                                                        string oappPath = "";
                                                        bool dotNetPublish = false;

                                                        if (inputArgs.Length > 2)
                                                            oappPath = inputArgs[2];

                                                        if (inputArgs.Length > 3 && inputArgs[3].ToLower() == "dotnetpublish")
                                                            dotNetPublish = true;

                                                        await STARCLI.PublishOAPPAsync(oappPath, dotNetPublish); //TODO: Implement PublishHappAsync ASAP!
                                                    }
                                                    break;
                                            }
                                        }

                                        await ShowSubCommandAsync(inputArgs, "hApp", "", STARCLI.LightWizardAsync, STARCLI.EditOAPPAsync, STARCLI.DeleteOAPPAsync, STARCLI.InstallOAPPAsync, STARCLI.UnInstallOAPPAsync, null, STARCLI.UnPublishOAPPAsync, STARCLI.ShowOAPPAsync, STARCLI.ListOAPPsCreatedByBeamedInAvatarAsync, STARCLI.ListAllOAPPsAsync, STARCLI.ListOAPPsInstalledForBeamedInAvatarAsync, STARCLI.SearchOAPPsAsync, ProviderType.Default, true, true);
                                        break;
                                    }

                                case "celestialspace":
                                    await ShowSubCommandAsync(inputArgs, "celestial space");
                                    break;

                                case "celestialbody":
                                    await ShowSubCommandAsync(inputArgs, "celestial body", "celestial bodies");
                                    break;

                                case "zome":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "holon":
                                    await ShowSubCommandAsync(inputArgs, "", "", null, null, STARCLI.DeleteHolonAsync, null, null, null, null, STARCLI.ShowHolonAsync, STARCLI.ListAllHolonsForForBeamedInAvatar, STARCLI.ListAllHolonsAsync, null, null, ProviderType.Default);
                                    break;

                                case "chapter":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "mission":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "quest":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "nft":
                                    await ShowNftSubCommandAsync(inputArgs);
                                    break;

                                case "geonft":
                                    await ShowGeoNftSubCommandAsync(inputArgs);
                                    break;

                                case "geohotspot":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "inventoryitem":
                                    await ShowSubCommandAsync(inputArgs);
                                    break;

                                case "avatar":
                                    await ShowAvatarSubCommandAsync(inputArgs);
                                    break;

                                case "karma":
                                    await ShowKarmaSubCommandAsync(inputArgs);
                                    break;

                                case "keys":
                                    await ShowKeysSubCommandAsync(inputArgs);
                                    break;

                                case "wallet":
                                    await ShowWalletSubCommandAsync(inputArgs);
                                    break;

                                case "map":
                                    await ShowMapSubCommandAsync(inputArgs);
                                    break;

                                case "seeds":
                                    await ShowSeedsSubCommandAsync(inputArgs);
                                    break;

                                case "data":
                                    await ShowDataSubCommandAsync(inputArgs);
                                    break;

                                case "oland":
                                    await ShowOlandSubCommandAsync(inputArgs);
                                    break;

                                case "search":
                                    CLIEngine.ShowMessage("Coming soon...");
                                    break;

                                case "onode":
                                    await ShowONODEConfigSubCommandAsync(inputArgs);
                                    break;

                                case "hypernet":
                                    await ShowHyperNETSubCommandAsync(inputArgs);
                                    break;

                                case "onet":
                                    await ShowONETSubCommandAsync(inputArgs);
                                    break;

                                case "config":
                                    await ShowConfigSubCommandAsync(inputArgs);
                                    break;

                                case "runcosmictests":
                                    {
                                        object oappTypeObj = null;
                                        OAPPType OAPPType = DEFAULT_OAPP_TYPE;
                                        OAPPTemplateType OAPPTemplateType = DEFAULT_OAPP_TEMPLATE_TYPE;
                                        Guid OAPPTemplateId = Guid.NewGuid();
                                        string dnaFolder = DEFAULT_DNA_FOLDER;
                                        string genesisFolder = DEFAULT_GENESIS_FOLDER;
                                        //string genesisNameSpace = DEFAULT_GENESIS_NAMESPACE;

                                        if (inputArgs.Length > 1)
                                        {
                                            if (Enum.TryParse(typeof(OAPPType), inputArgs[2], true, out oappTypeObj))
                                                OAPPType = (OAPPType)oappTypeObj;
                                        }

                                        if (inputArgs.Length > 2)
                                            dnaFolder = inputArgs[1];

                                        if (inputArgs.Length > 3)
                                            genesisFolder = inputArgs[2];

                                        if (OAPPType == DEFAULT_OAPP_TYPE)
                                            CLIEngine.ShowWorkingMessage($"OAPPType Not Specified, Using Default: {Enum.GetName(typeof(OAPPType), OAPPType)}");
                                        else
                                            CLIEngine.ShowWorkingMessage($"OAPPType Specified: {Enum.GetName(typeof(OAPPType), OAPPType)}");

                                        if (dnaFolder == DEFAULT_DNA_FOLDER)
                                            CLIEngine.ShowWorkingMessage($"DNAFolder Not Specified, Using Default: {dnaFolder}");
                                        else
                                            CLIEngine.ShowWorkingMessage($"DNAFolder Specified: {dnaFolder}");

                                        if (genesisFolder == DEFAULT_GENESIS_FOLDER)
                                            CLIEngine.ShowWorkingMessage($"GenesisFolder Not Specified, Using Default: {genesisFolder}");
                                        else
                                            CLIEngine.ShowWorkingMessage($"GenesisFolder Specified: {genesisFolder}");

                                        await STARCLI.RunCOSMICTests(OAPPType, OAPPTemplateType, OAPPTemplateId, dnaFolder, genesisFolder);
                                    }
                                    break;

                                case "runoasisapitests":
                                    await STARCLI.RunOASISAPTests();
                                    break;

                                default:
                                    CLIEngine.ShowErrorMessage("Command Unknown.");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        //ConsoleKeyInfo keyInfo = Console.ReadKey();

                        //if (keyInfo.KeyChar == 'c' && keyInfo.Modifiers == ConsoleModifiers.Control)
                        //    exit = CLIEngine.GetConfirmation("STAR: Are you sure you wish to exit?");
                    }
                }
                catch (Exception ex)
                {
                    OASISErrorHandling.HandleError($"An unknown error occured in STARCLI.ReadyPlayerOne. Reason: {ex}", ex);
                }
            }
            while (!exit);

            CLIEngine.ShowMessage("Thank you for using STAR & The OASIS! We hope you enjoyed your stay, have a nice day! :)");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static async Task ShowSubCommandAsync(string[] inputArgs, 
            string subCommand = "",
            string subCommandPlural = "",
            Func<ProviderType, Task> createPredicate = null, 
            Func<string, ProviderType, Task> updatePredicate = null, 
            Func<string, ProviderType, Task> deletePredicate = null, 
            Func<string, ProviderType, Task> installPredicate = null,
            Func<string, ProviderType, Task> uninstallPredicate = null,
            Func<string, ProviderType, Task> publishPredicate = null,
            Func<string, ProviderType, Task> unpublishPredicate = null,
            Func<string, ProviderType, Task> showPredicate = null,
            Func<ProviderType, Task> listForBeamedInAvatarPredicate = null,
            Func<ProviderType, Task> listAllPredicate = null,
            Func<ProviderType, Task> listInstalledPredicate = null,
            Func<ProviderType, Task> searchPredicate = null, 
            ProviderType providerType = ProviderType.Default,
            bool isOAPPOrHapp = false,
            bool ishApp = false,
            bool showCreate = true,
            bool showUpdate = true,
            bool showDelete = true)
            //string deleteName = "delete")
        {
            if (string.IsNullOrEmpty(subCommand))
                subCommand = inputArgs[0];

            if (inputArgs.Length > 1)
            {
                string id = "";

                if (inputArgs.Length > 2)
                    id = inputArgs[2];

                //Guid id = Guid.Empty;

                //if (inputArgs.Length > 2)
                //{
                //    if (!Guid.TryParse(inputArgs[2], out id))
                //        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                //}

                switch (inputArgs[1].ToLower())
                {
                    case "create":
                        {
                            if (showCreate)
                            {
                                if (createPredicate != null)
                                    await createPredicate(providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else
                                CLIEngine.ShowMessage("Command not supported.");
                        }
                        break;

                    case "update":
                        {
                            if (showUpdate)
                            {
                                if (updatePredicate != null)
                                    await updatePredicate(id, providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else
                                CLIEngine.ShowMessage("Command not supported.");
                        }
                        break;

                    case "delete":
                        {
                            if (showDelete)
                            {
                                if (deletePredicate != null)
                                    await deletePredicate(id, providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else
                                CLIEngine.ShowMessage("Command not supported.");
                        }
                        break;

                    case "install":
                        {
                            if (isOAPPOrHapp)
                            {
                                if (installPredicate != null)
                                    await installPredicate(id, providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else
                                CLIEngine.ShowMessage("Command not supported.");
                        }
                        break;

                    case "uninstall":
                        {
                            if (isOAPPOrHapp)
                            {
                                if (uninstallPredicate != null)
                                    await uninstallPredicate(id, providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else
                                CLIEngine.ShowMessage("Command not supported.");
                        }
                        break;

                    case "publish":
                        {
                            if (publishPredicate != null)
                                await publishPredicate(id, providerType);
                            else
                                CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "unpublish":
                        {
                            if (unpublishPredicate != null)
                                await unpublishPredicate(id, providerType);
                            else
                                CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "show":
                        {
                            if (showPredicate != null)
                                await showPredicate(id, providerType);
                            else
                                CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "list":
                        {
                            if (inputArgs.Length > 2 && inputArgs[2] != null && inputArgs[2].ToLower() == "all")
                            {
                                if (listAllPredicate != null)
                                    await listAllPredicate(providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                            else if (inputArgs.Length > 2 && inputArgs[2] != null && inputArgs[2].ToLower() == "installed")
                            {
                                if (isOAPPOrHapp)
                                {
                                    if (listInstalledPredicate != null)
                                        await listInstalledPredicate(providerType);
                                    else
                                        CLIEngine.ShowMessage("Coming Soon...");
                                }
                                else
                                    CLIEngine.ShowMessage("Command not supported.");
                            }
                            else
                            {
                                if (listForBeamedInAvatarPredicate != null)
                                    await listForBeamedInAvatarPredicate(providerType);
                                else
                                    CLIEngine.ShowMessage("Coming Soon...");
                            }
                        }
                        break;

                    case "search":
                        {
                            if (searchPredicate != null)
                                await searchPredicate(providerType);
                            else
                                CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(subCommandPlural))
                    subCommandPlural = $"{subCommand}'s";

                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                CLIEngine.ShowMessage($"{subCommand.ToUpper()} SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");

                int commandSpace = 15;
                //int paramSpace = 2;
                //string commandSpace = " ";
                string paramSpace = "  ";

                if (isOAPPOrHapp)
                {
                    //commandSpace = "           ";
                    commandSpace = 20;
                    paramSpace = "                   ";
                }

                if (showCreate)
                    CLIEngine.ShowMessage(string.Concat("    create".PadRight(commandSpace), "{id/name}", paramSpace, "Create a ", subCommand, "."), ConsoleColor.Green, false);

                if (showUpdate)
                    CLIEngine.ShowMessage(string.Concat("    update".PadRight(commandSpace), "{id/name}", paramSpace, "Update an existing ", subCommand, " for the given {id} or {name}."), ConsoleColor.Green, false);

                if (showDelete)
                    CLIEngine.ShowMessage(string.Concat("    delete".PadRight(commandSpace), "{id/name}", paramSpace, "Delete an existing ", subCommand, " for the given {id} or {name}."), ConsoleColor.Green, false);

                if (isOAPPOrHapp)
                {
                    CLIEngine.ShowMessage(string.Concat("    install".PadRight(commandSpace), "{id/name}", paramSpace, "Install a ", subCommand, " for the given {id} or {name}."), ConsoleColor.Green, false);
                    CLIEngine.ShowMessage(string.Concat("    uninstall".PadRight(commandSpace), "{id/name}", paramSpace, "Uninstall a ", subCommand, " for the given {id} or {name}."), ConsoleColor.Green, false);
                }

                if (isOAPPOrHapp)
                {
                    if (ishApp)
                        CLIEngine.ShowMessage(string.Concat("    publish".PadRight(commandSpace), "{hAppPath} [publishDotNet]  Publish a ", subCommand, " for the given {hAppPath}. If the flag [publishDotNet] is specefied it will first do a dotnet publish before publishing to STARNET."), ConsoleColor.Green, false);
                    else
                        CLIEngine.ShowMessage(string.Concat("    publish".PadRight(commandSpace), "{oappPath} [publishDotNet]  Publish a ", subCommand, " for the given {oappPath}. If the flag [publishDotNet] is specefied it will first do a dotnet publish before publishing to STARNET."), ConsoleColor.Green, false);
                }
                else
                    CLIEngine.ShowMessage(string.Concat("    publish".PadRight(commandSpace), "{id/name}", paramSpace, "Publish a ", subCommand, " to STARNET for the given {id} or {name}."), ConsoleColor.Green, false);

                CLIEngine.ShowMessage(string.Concat("    unpublish".PadRight(commandSpace), "{id/name}", paramSpace, "Unpublish a ", subCommand, " from STARNET for the given {id} or {name}."), ConsoleColor.Green, false);
                CLIEngine.ShowMessage(string.Concat("    list".PadRight(commandSpace), "[all]", paramSpace, "    List all ", subCommandPlural, " that have been created. If {all} is omitted it will list only your ", subCommandPlural, "'s otherwise it will list all published ", subCommandPlural, "'s as well as yours."), ConsoleColor.Green, false);

                if (isOAPPOrHapp)
                    CLIEngine.ShowMessage(string.Concat("    list installed".PadRight(commandSpace), "{id/name}", paramSpace, "List all ", subCommandPlural, "' installed for the current beamed in avatar."), ConsoleColor.Green, false);

                CLIEngine.ShowMessage(string.Concat("    search".PadRight(commandSpace), "{id/name}", paramSpace, "Searches the ", subCommandPlural, " for the given search critera."), ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowAvatarSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                //Guid id = Guid.Empty;

                //if (inputArgs.Length > 2)
                //{
                //    if (!Guid.TryParse(inputArgs[2], out id))
                //        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                //}

                switch (inputArgs[1].ToLower())
                {
                    case "beamin":
                        {
                            if (STAR.BeamedInAvatar == null)
                                await STARCLI.BeamInAvatar();
                            else
                                CLIEngine.ShowErrorMessage($"Avatar {STAR.BeamedInAvatar.Username} Already Beamed In. Please Beam Out First!");
                        }
                        break;

                    case "beamout":
                        {
                            if (STAR.BeamedInAvatar != null)
                            {
                                OASISResult<IAvatar> avatarResult = await STAR.BeamedInAvatar.BeamOutAsync();

                                if (avatarResult != null && !avatarResult.IsError && avatarResult.Result != null)
                                {
                                    STAR.BeamedInAvatar = null;
                                    STAR.BeamedInAvatarDetail = null;
                                    CLIEngine.ShowSuccessMessage("Avatar Successfully Beamed Out! We Hope You Enjoyed Your Time In The OASIS! Please Come Again! :)");
                                }
                                else
                                    CLIEngine.ShowErrorMessage($"Error Beaming Out Avatar: {avatarResult.Message}");
                            }
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "whoisbeamedin":
                        {
                            if (STAR.BeamedInAvatar != null)
                                CLIEngine.ShowMessage($"Avatar {STAR.BeamedInAvatar.Username} Beamed In On {STAR.BeamedInAvatar.LastBeamedIn} And Last Beamed Out On {STAR.BeamedInAvatar.LastBeamedOut}. They Are Level {STAR.BeamedInAvatarDetail.Level} With {STAR.BeamedInAvatarDetail.Karma} Karma.", ConsoleColor.Green);
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "show":
                        {
                            if (inputArgs.Length > 2)
                            {
                                if (inputArgs[2] == "me")
                                    STARCLI.ShowAvatar(STAR.BeamedInAvatar, STAR.BeamedInAvatarDetail);
                                else
                                    await STARCLI.ShowAvatar(inputArgs[2]);
                            }
                            else
                                await STARCLI.ShowAvatar();
                        }
                        break;


                    case "edit":
                        {
                            if (STAR.BeamedInAvatar != null)
                                CLIEngine.ShowMessage("Coming soon...");
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "list":
                        {
                            //STARCLI.ListAvat
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "search":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"AVATAR SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    beamin                       Beam in (log in).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    beamout                      Beam out (log out).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    whoisbeamedin                Display who is currently beamed in (if any) and the last time they beamed in and out.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    show me                      Display the currently beamed in avatar details (if any).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    show          {id/username}  Shows the details for the avatar for the given {id} or {username}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    edit                         Edit the currently beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list                         Lists all avatars.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    search                       Seach avatars that match the given seach parameters (public fields only such as level, karma, username & any fields the player has set to public).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowOAPPTemplateSubCommandsAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 2)
            {
                //Guid id = Guid.Empty;

                //if (inputArgs.Length > 3)
                //{
                //    if (!Guid.TryParse(inputArgs[2], out id))
                //        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                //}

                switch (inputArgs[2].ToLower())
                {
                    case "create":
                        {
                            if (STAR.BeamedInAvatar == null)
                                await STARCLI.BeamInAvatar();
                            else
                                CLIEngine.ShowErrorMessage($"Avatar {STAR.BeamedInAvatar.Username} Already Beamed In. Please Beam Out First!");
                        }
                        break;

                    case "update":
                        {
                            if (STAR.BeamedInAvatar != null)
                            {
                                OASISResult<IAvatar> avatarResult = await STAR.BeamedInAvatar.BeamOutAsync();

                                if (avatarResult != null && !avatarResult.IsError && avatarResult.Result != null)
                                {
                                    STAR.BeamedInAvatar = null;
                                    STAR.BeamedInAvatarDetail = null;
                                    CLIEngine.ShowSuccessMessage("Avatar Successfully Beamed Out! We Hope You Enjoyed Your Time In The OASIS! Please Come Again! :)");
                                }
                                else
                                    CLIEngine.ShowErrorMessage($"Error Beaming Out Avatar: {avatarResult.Message}");
                            }
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "delete":
                        {
                            if (STAR.BeamedInAvatar != null)
                                CLIEngine.ShowMessage($"Avatar {STAR.BeamedInAvatar.Username} Beamed In On {STAR.BeamedInAvatar.LastBeamedIn} And Last Beamed Out On {STAR.BeamedInAvatar.LastBeamedOut}. They Are Level {STAR.BeamedInAvatarDetail.Level} With {STAR.BeamedInAvatarDetail.Karma} Karma.", ConsoleColor.Green);
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "install":
                        {
                            if (inputArgs.Length > 2)
                            {
                                if (inputArgs[2] == "me")
                                    STARCLI.ShowAvatar(STAR.BeamedInAvatar, STAR.BeamedInAvatarDetail);
                                else
                                    await STARCLI.ShowAvatar(inputArgs[2]);
                            }
                            else
                                await STARCLI.ShowAvatar();
                        }
                        break;


                    case "uninstall":
                        {
                            if (STAR.BeamedInAvatar != null)
                                CLIEngine.ShowMessage("Coming soon...");
                            else
                                CLIEngine.ShowErrorMessage("No Avatar Is Beamed In!");
                        }
                        break;

                    case "publish":
                        {
                            //STARCLI.ListAvat
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "unpublish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "show":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"OAPP TEMPLATE SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    create                         Creates a OAPP template.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    update          {id/username}  Updates a OAPP template for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    delete          {id/username}  Deletes a OAPP template for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    install         {id/username}  Installs a OAPP template for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    uninstall       {id/username}  Uninstalls a OAPP template for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    publish         {id/username}  Publishes a OAPP template to the STARNET store for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    unpublish       {id/username}  Unpublishes a OAPP template from the STARNET store for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    show            {id/username}  Shows a OAPP template for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list            [all]          List all OAPP templates that have been created.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list installed  {id/username}  List all OAPP templates installed for the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    search          [all]          Searches the OAPP templates for the given search critera.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }


        /*
        private static async Task ShowHolonSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                Guid id = Guid.Empty;

                if (inputArgs.Length > 2)
                {
                    if (!Guid.TryParse(inputArgs[2], out id))
                        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                }

                switch (inputArgs[1].ToLower())
                {
                    case "create":
                        {

                        }
                        break;

                    case "update":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "delete":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "publish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "unpublish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "show":
                        {
                            await STARCLI.ShowNFTAsync(id);
                        }
                        break;

                    case "list":
                        {
                            if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
                                await STARCLI.ListAllNFTsAsync();
                            else
                                await STARCLI.ListNFTsForBeamedInAvatarAsync();
                        }
                        break;

                    case "search":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"NFT SUBCOMMANDS:", ConsoleColor.Green);
                CLIEngine.ShowMessage("    create json = {holonJSONFile}  Creates/Saves a holon from the given {holonJSONFile}.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    create wiz                     Starts the Create Holon Wizard.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    update        {id/name}        Update an existing holon for the given {id} or {name} (can upload a holon.cs file containing custom code/logic/functions which is then shareable with other OAPP's).", ConsoleColor.Green);
                CLIEngine.ShowMessage("    delete        {id/name}        Deletes a holon for the given {id} or {name}.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    publish       {id/name}        Publishes a holon for the given {id} or {name} to the STARNET store so others can use in their own OAPP's/hApp's etc.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    unpublish     {id/name}        Unpublishes a holon for the given {id} or {name} from the STARNET store.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    show          {id/name}        Shows the holon for the given {id} or {name}.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    list          {all}            List all holon's that have been created. If {all} is omitted it will list only your holon's otherwise it will list all published holon's as well as yours.", ConsoleColor.Green);
                CLIEngine.ShowMessage("    search                         Search for holon's that match certain criteria and belong to the current beamed in avatar.", ConsoleColor.Green);
                Console.WriteLine("");
            }
        }*/

        private static async Task ShowNftSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                //Guid id = Guid.Empty;

                //if (inputArgs.Length > 2)
                //{
                //    if (!Guid.TryParse(inputArgs[2], out id))
                //        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                //}

                switch (inputArgs[1].ToLower())
                {
                    case "mint":
                        await STARCLI.MintNFTAsync();
                        break;

                    case "send":
                        await STARCLI.SendNFTAsync();
                        break;

                    case "update":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "burn":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "publish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "unpublish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "show":
                        {
                            await STARCLI.ShowNFTAsync();
                        }
                        break;

                    case "list":
                        {
                            if (inputArgs.Length > 2 && inputArgs[2] != null && inputArgs[2].ToLower() == "all")
                                await STARCLI.ListAllNFTsAsync();
                            else
                                await STARCLI.ListNFTsForBeamedInAvatarAsync();
                        }
                        break;

                    case "search":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"NFT SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    mint                  Mints a OASIS NFT for the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    update     {id/name}  Updates a OASIS NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    burn                  Burn's a OASIS NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    send                  Send a OASIS NFT for the given {id} or {name} to another wallet cross-chain.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    publish    {id/name}  Publishes a OASIS NFT for the given {id} or {name} to the STARNET store so others can use in their own geo-nft's etc.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    unpublish  {id/name}  Unpublishes a OASIS NFT for the given {id} or {name} from the STARNET store.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    show       {id/name}  Shows the OASIS NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list       [all]      List all OASIS NFT's that have been created. If the [all] flag is omitted it will list only your NFT's otherwise it will list all published NFT's as well as yours.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    search                Search for OASIS NFT's that match certain criteria and belong to the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowGeoNftSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                //Guid id = Guid.Empty;

                //if (inputArgs.Length > 2)
                //{
                //    if (!Guid.TryParse(inputArgs[2], out id))
                //        CLIEngine.ShowErrorMessage($"The id ({inputArgs[2]}) passed in is not a valid GUID!");
                //}

                switch (inputArgs[1].ToLower())
                {
                    case "mint":
                        await STARCLI.MintGeoNFTAsync();
                        break;

                    case "send":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "place":
                        await STARCLI.PlaceGeoNFTAsync();
                        break;

                    case "update":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "burn":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "publish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                            //await STARCLI.Pub
                        }
                        break;

                    case "unpublish":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    case "show":
                        {
                            await STARCLI.ShowGeoNFTAsync();
                        }
                        break;

                    case "list":
                        {
                            if (inputArgs.Length > 2 && inputArgs[2] != null && inputArgs[2].ToLower() == "all")
                                await STARCLI.ListAllGeoNFTsAsync();
                            else
                                await STARCLI.ListAllGeoNFTsForBeamedInAvatarAsync();
                        }
                        break;

                    case "search":
                        {
                            CLIEngine.ShowMessage("Coming soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"GEONFT SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    mint                  Mints a OASIS Geo-NFT and places in Our World/AR World for the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    update     {id/name}  Updates a OASIS Geo-NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    burn       {id/name}  Burn's a OASIS Geo-NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    send       {id/name}  Send a OASIS Geo-NFT for the given {id} or {name} to another wallet cross-chain.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    publish    {id/name}  Publishes a OASIS Geo-NFT for the given {id} or {name} to the STARNET store so others can use in their own geo-nft's etc.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    unpublish  {id/name}  Unpublishes a OASIS Geo-NFT for the given {id} or {name} from the STARNET store.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    show       {id/name}  Shows the OASIS Geo-NFT for the given {id} or {name}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list       [all]      List all OASIS Geo-NFT's that have been created. If the [all] flag is omitted it will list only your Geo-NFT's otherwise it will list all published Geo-NFT's as well as yours.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    search                Search for OASIS Geo-NFT's that match certain criteria and belong to the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowKeysSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "link":
                        await STARCLI.MintGeoNFTAsync();
                        break;

                    case "list":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"KEYS SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    link                  Links a OASIS Provider Key to the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list                  Shows the keys for the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowKarmaSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "list":
                        STAR.OASISAPI.Avatar.ShowKarmaThresholds();
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"KARMA SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    list                  Display the karma thresholds.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowWalletSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "list":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"WALLET SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    list                  Shows the wallets for the current beamed in avatar.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowMapSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "setprovider":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "draw3dobject":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "draw2dsprite":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "draw2dspriteonhud":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "draw2dspriteonhud":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"MAP SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    map setprovider            {mapProviderType}                 Sets the current {mapProviderType}.");
                Console.WriteLine("    map draw3dobject           {3dObjectPath} {x} {y}            Draws a 3D object on the map at {x/y} co-ordinates for the given file {3dobjectPath}.");
                Console.WriteLine("    map draw2dsprite           {2dSpritePath} {x} {y}            Draws a 2d sprite on the map at {x/y} co-ordinates for the given file {2dSpritePath}.");
                Console.WriteLine("    map draw2dspriteonhud      {2dSpritePath}                    Draws a 2d sprite on the HUD for the given file {2dSpritePath}.");
                Console.WriteLine("    map zoomOut                                                  Zoom the map out.");
                Console.WriteLine("    map zoomIn                                                   Zoom the map in.");
                Console.WriteLine("    map pamLeft                                                  Pam the map left.");
                Console.WriteLine("    map pamLeft                                                  Pam the map left.");
                Console.WriteLine("    map pamRight                                                 Pam the map right.");
                Console.WriteLine("    map pamUp                                                    Pam the map left.");
                Console.WriteLine("    map pamDown                                                  Pam the map down.");
                Console.WriteLine("    map pamToGeoNFT             {GeoNFT id/name}                 Pam the map to the location of the given GeoNFT.");
                Console.WriteLine("    map pamToGeoHotSpot         {GeoHotSpot id/name}             Pam the map to the location of the given GeoHotSpot.");
                Console.WriteLine("    map pamToOAPP               {OAPP id/name}                   Pam the map to the location of the given OAPP.");
                Console.WriteLine("    map pamToCoOrds             {x} {y}                          Pam the map to the location of the given {x} and {y} coordinates.");
                Console.WriteLine("    map selectBuildingOnMap     {building id}                    Selects the given building on the map.");
                //Console.WriteLine("    map highlightBuildingOnMap  {building id}                    Highlight the given building on the map.");
                Console.WriteLine("    map drawRouteOnMap          {startX} {startY} {endX} {endY}  Draw a route on the map.");
                Console.WriteLine("    map drawRouteOnMap          {startX} {startY} {endX} {endY}  Draw a route on the map.");

                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowDataSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "save":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "load":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "delete":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "list":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"DATA SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    data save    {key} {value}  Saves data for the given {key} and {value} to the currently beamed in avatar.");
                Console.WriteLine("    data load    {key}          Loads data for the given {key} for the currently beamed in avatar.");
                Console.WriteLine("    data delete  {key}          Deletes data for the given {key} for the currently beamed in avatar.");
                Console.WriteLine("    data list                   Lists all data for the currently beamed in avatar.");
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowSeedsSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "balance":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "organisations":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "organisation":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "pay":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "donate":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "reward":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "invite":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "accept":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "qrcode":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"SEEDS SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    seeds balance        {telosAccountName/avatarId}  Get's the balance of your SEEDS account.");
                Console.WriteLine("    seeds organisations                               Get's a list of all the SEEDS organisations.");
                Console.WriteLine("    seeds organisation   {organisationName}           Get's a organisation for the given {organisationName}.");
                Console.WriteLine("    seeds pay            {telosAccountName/avatarId}  Pay using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds donate         {telosAccountName/avatarId}  Donate using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds reward         {telosAccountName/avatarId}  Reward using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds invite         {telosAccountName/avatarId}  Send invite to join SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds accept         {telosAccountName/avatarId}  Accept the invite to join SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds qrcode         {telosAccountName/avatarId}  Generate a sign-in QR code using either your {telosAccountName} or {avatarId}.");

                //CLIEngine.ShowMessage("    balance        {telosAccountName/avatarId}  Get's the balance of your SEEDS account.", ConsoleColor.Green, false);
                //CLIEngine.ShowMessage("    organisations                               Get's a list of all the SEEDS organisations.", ConsoleColor.Green, false);
                //CLIEngine.ShowMessage("    organisation   {organisationName}           Get's a list of all the SEEDS organisations.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowOlandSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "price":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "purchase":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "load":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "save":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "delete":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    case "list":
                        CLIEngine.ShowMessage("Coming soon...");
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"OLAND SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    price                  Get the current OLAND price.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    purchase               Purchase OLAND for Our World/OASIS.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    load      {id}         Load a OLAND for the given {id}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    save      {id}         Save a OLAND for the given {id}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    delete    {id}         Delete a OLAND for the given {id}.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    list      {all}        If [all] is omitted it will list all OLAND for the given beamed in avatar, otherwise it will list all OLAND for all avatars.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowConfigSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "cosmicdetailedoutput":
                        { 
                            if (inputArgs.Length > 2)
                            {
                                switch (inputArgs[2].ToLower())
                                {
                                    case "enabled":
                                        {
                                            STAR.IsDetailedCOSMICOutputsEnabled = true;
                                            CLIEngine.ShowMessage("Detailed COSMIC Output Enabled.");
                                        }
                                        break;

                                    case "disabled":
                                        {
                                            STAR.IsDetailedCOSMICOutputsEnabled = false;
                                            CLIEngine.ShowMessage("Detailed COSMIC Output Disabled.");
                                        }
                                        break;

                                    case "status":
                                        {
                                            if (STAR.IsDetailedCOSMICOutputsEnabled)
                                                CLIEngine.ShowSuccessMessage("COSMIC Detailed Output Status: Enabled.");
                                            else
                                                CLIEngine.ShowSuccessMessage("COSMIC Detailed Output Status: Disabled.");
                                        }
                                        break;

                                    default:
                                        CLIEngine.ShowErrorMessage("Command Unknown.");
                                        break;
                                }
                            }
                            else
                            {
                                if (STAR.IsDetailedCOSMICOutputsEnabled)
                                    CLIEngine.ShowSuccessMessage("COSMIC Detailed Output Status: Enabled.");
                                else
                                    CLIEngine.ShowSuccessMessage("COSMIC Detailed Output Status: Disabled.");
                            }
                        }
                        break;

                    case "starstatusdetailedoutput":
                        {
                            if (inputArgs.Length > 2)
                            {
                                switch (inputArgs[2].ToLower())
                                {
                                    case "enabled":
                                        {
                                            STAR.IsDetailedCOSMICOutputsEnabled = true;
                                            CLIEngine.ShowSuccessMessage("STAR Detailed Status Enabled.");
                                        }
                                        break;

                                    case "disabled":
                                        {
                                            STAR.IsDetailedCOSMICOutputsEnabled = false;
                                            CLIEngine.ShowSuccessMessage("STAR Detailed Status Disabled.");
                                        }
                                        break;

                                    case "status":
                                        {
                                            if (STAR.IsDetailedCOSMICOutputsEnabled)
                                                CLIEngine.ShowMessage("STAR Detailed Status: Enabled.");
                                            else
                                                CLIEngine.ShowMessage("STAR Detailed Status: Disabled.");
                                        }
                                        break;

                                    default:
                                        CLIEngine.ShowErrorMessage("Command Unknown.");
                                        break;
                                }
                            }
                            else
                            {
                                if (STAR.IsDetailedCOSMICOutputsEnabled)
                                    CLIEngine.ShowMessage("STAR Detailed Status: Enabled.");
                                else
                                    CLIEngine.ShowMessage("STAR Detailed Status: Disabled.");
                            }
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"CONFIG SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    cosmicdetailedoutput     [enable/disable/status] Enables/disables COSMIC Detailed Output.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    starstatusdetailedoutput [enable/disable/status] Enables/disables STAR ODK Detailed Output.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowONODEConfigSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "start":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "stop":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "status":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "config":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "providers":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "startprovider":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "stopprovider":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"ONODE SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    start                          Starts a OASIS Node (ONODE) and registers it on the OASIS Network (ONET).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    stop                           Stops a OASIS Node (ONODE).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    status                         Shows stats for this ONODE.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    config                         Opens the ONODE's OASISDNA to allow changes to be made (you will need to stop and start the ONODE for changes to apply).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    providers                      Shows what OASIS Providers are running for this ONODE.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    startprovider  {ProviderName}  Starts a given provider.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    stopprovider   {ProviderName}  Stops a given provider.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowHyperNETSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "start":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "stop":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "status":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "config":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"HOLONET P2P HYPERNET SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    start   Starts the HoloNET P2P HyperNET Service.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    stop    Stops the HoloNET P2P HyperNET Service.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    status  Shows stats for the HoloNET P2P HyperNET Service.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    config  Opens the HyperNET's DNA to allow changes to be made (you will need to stop and start the HyperNET Service for changes to apply).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        private static async Task ShowONETSubCommandAsync(string[] inputArgs)
        {
            if (inputArgs.Length > 1)
            {
                switch (inputArgs[1].ToLower())
                {
                    case "status":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    case "providers":
                        {
                            CLIEngine.ShowMessage("Coming Soon...");
                        }
                        break;

                    default:
                        CLIEngine.ShowErrorMessage("Command Unknown.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("");
                CLIEngine.ShowMessage($"ONET SUBCOMMANDS:", ConsoleColor.Green);
                Console.WriteLine("");
                CLIEngine.ShowMessage("    status     Shows stats for the OASIS Network (ONET).", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("    providers  Shows what OASIS Providers are running across the ONET and on what ONODE's.", ConsoleColor.Green, false);
                CLIEngine.ShowMessage("More Coming Soon...", ConsoleColor.Green);
            }
        }

        //private static void ShowOappSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("celestial body", "celestial bodies");
        //}

        //private static void ShowHappSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("celestial body", "celestial bodies");
        //}

        //private static void ShowCelestialBodySubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("celestial body", "celestial bodies");
        //}

        //private static void ShowCelestialSpaceSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("celestial space");
        //}

        //private static void ShowZomeSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("zome");
        //}

        //private static void ShowHolonSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("holon");
        //}

        //private static void ShowMissionSubCommand(string[] inputArgs)
        //{
        //    if (inputArgs.Length > 1)
        //    {
        //        switch (inputArgs[1].ToLower())
        //        {
        //            case "create":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "update":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "delete":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "publish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "unpublish":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "show":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;

        //            case "list":
        //                {
        //                    if (inputArgs[2] != null && inputArgs[2].ToLower() == "all")
        //                        CLIEngine.ShowMessage("Coming soon...");

        //                    else
        //                        CLIEngine.ShowMessage("Coming soon...");
        //                }
        //                break;

        //            case "search":
        //                CLIEngine.ShowMessage("Coming soon...");
        //                break;
        //        }
        //    }
        //    else
        //        ShowSubCommand("mission");
        //}

        private static async Task SaveHolonWizard()
        {
            //TODO: Finish implementing...
            CLIEngine.ShowMessage("Coming Soon...");
        }

        //private static async Task LightWizard()
        //{
        //    OASISResult<CoronalEjection> lightResult = null;
        //    string OAPPName = CLIEngine.GetValidInput("What is the name of the OAPP?");
        //    object value = CLIEngine.GetValidInputForEnum("What type of OAPP do you wish to create?", typeof(OAPPType));

        //    if (value != null)
        //    {
        //        OAPPType OAPPType = (OAPPType)value;

        //        value = CLIEngine.GetValidInputForEnum("What type of GenesisType do you wish to create?", typeof(GenesisType));

        //        if (value != null)
        //        {
        //            GenesisType genesisType = (GenesisType)value;
        //            string dnaFolder = "";

        //            //if (CLIEngine.GetConfirmation("Do you wish to create the CelestialBody/Zomes/Holons DNA now? (Enter 'n' if you already have a folder containing the DNA)."))
        //            //{
        //            //    //string zomeName = CLIEngine.GetValidInput("What is the name of the Zome (collection of Holons)?");
        //            //    //string holonName = CLIEngine.GetValidInput("What is the name of the Holon (OASIS Data Object)?");
        //            //    //string propName = CLIEngine.GetValidInput("What is the name of the Field/Property?");
        //            //    //object propType = CLIEngine.GetValidInputForEnum("What is the type of the Field/Property?", typeof(HolonPropType));

        //            //    //TODO:Come back to this.
        //            //}
        //            //else
        //                dnaFolder = CLIEngine.GetValidPath("What is the path to the CelestialBody/Zomes/Holons DNA?");

        //            if (Directory.Exists(dnaFolder) && Directory.GetFiles(dnaFolder).Length > 0)
        //            {
        //                string genesisFolder = CLIEngine.GetValidPath("What is the path to the GenesisFolder?");
        //                string genesisNamespace = CLIEngine.GetValidInput("What is the Genesis Namespace?");
        //                Guid parentId = Guid.Empty;

        //                if (CLIEngine.GetConfirmation("Does this OAPP belong to another CelestialBody?"))
        //                {
        //                    parentId = CLIEngine.GetValidInputForGuid("What is the Id (GUID) of the parent CelestialBody?");

        //                    Console.WriteLine("");
        //                    CLIEngine.ShowWorkingMessage("Generating OAPP...");
        //                    lightResult = await STAR.LightAsync(OAPPName, OAPPType, genesisType, dnaFolder, genesisFolder, genesisNamespace, parentId);
        //                }
        //                else
        //                {
        //                    Console.WriteLine("");
        //                    CLIEngine.ShowWorkingMessage("Generating OAPP...");
        //                    lightResult = await STAR.LightAsync(OAPPName, OAPPType, genesisType, dnaFolder, genesisFolder, genesisNamespace);
        //                }

        //                if (lightResult != null)
        //                {
        //                    if (!lightResult.IsError && lightResult.Result != null)
        //                        CLIEngine.ShowSuccessMessage($"OAPP Successfully Generated. ({lightResult.Message})");
        //                    else
        //                        CLIEngine.ShowErrorMessage($"Error Occured: {lightResult.Message}");
        //                }
        //            }
        //            else
        //                CLIEngine.ShowErrorMessage($"The DnaFolder {dnaFolder} Is Not Valid. It Does Mot Contain Any Files!");
        //        }
        //    }
        //}

        ///// <summary>
        ///// This is a good example of how to programatically interact with STAR including scripting etc...
        ///// </summary>
        ///// <param name="celestialBodyDNAFolder"></param>
        ///// <param name="geneisFolder"></param>
        ///// <returns></returns>
        //private static async Task RunCOSMICTests(OAPPType OAPPType, string celestialBodyDNAFolder, string geneisFolder)
        //{
        //    CLIEngine.ShowWorkingMessage("BEGINNING STAR ODK/COSMIC TEST'S...");

        //    OASISResult<CoronalEjection> result = await GenerateZomesAndHolons("Zomes And Holons Only", OAPPType, celestialBodyDNAFolder, Path.Combine(geneisFolder, "ZomesAndHolons"), "NextGenSoftware.OASIS.OAPPS.ZomesAndHolonsOnly");

        //    //Passing in null for the ParentCelestialBody will default it to the default planet (Our World).
        //    result = await GenerateCelestialBody("The Justice League Academy", null, OAPPType, GenesisType.Moon, celestialBodyDNAFolder, Path.Combine(geneisFolder, "JLA"), "NextGenSoftware.OASIS.OAPPS.JLA");

        //    // Currenly the JLA Moon and Our World Planet share the same Zome/Holon DNA (celestialBodyDNAFolder) but they can also have their own zomes/holons if they wish...
        //    // TODO: In future you will also be able to define the full CelestialBody DNA seperatley (cs/json) for each planet, moon,  etc where they can also define additional meta data for the moon/planet/ as well as their own zomes/holons like we have now, plus they can also refer to existing holons/zomes either in a folder (like we have now) or in STARNET Library using the GUID.
        //    // They will still be able to use a shared zomes/holons DNA folder as it is now if they wish or a combo of the two approaches...

        //    if (result != null && !result.IsError && result.Result != null && result.Result.CelestialBody != null)
        //    {
        //        _jlaMoon = (Moon)result.Result.CelestialBody;
        //        await LoadCelestialBodyAsync(_jlaMoon, "The Justice League Academy Moon");
        //        await LoadHolonAsync(_jlaMoon.Id, "The Justice League Academy Moon");
        //    }

        //    //Passing in null for the ParentCelestialBody will default it to the default Star (Our Sun Sol).
        //    result = await GenerateCelestialBody("Our World", null, OAPPType, GenesisType.Planet, celestialBodyDNAFolder, Path.Combine(geneisFolder, "Our World"), "NextGenSoftware.OASIS.OAPPS.OurWorld");

        //    if (result != null && !result.IsError && result.Result != null && result.Result.CelestialBody != null)
        //    {
        //        _superWorld = (Planet)result.Result.CelestialBody;

        //        result.Result.CelestialBody.OnHolonLoaded += CelestialBody_OnHolonLoaded;
        //        result.Result.CelestialBody.OnHolonSaved += CelestialBody_OnHolonSaved;
        //        result.Result.CelestialBody.OnZomeError += CelestialBody_OnZomeError;

        //        CLIEngine.ShowWorkingMessage("Loading Our World Zomes & Holons...");
        //        OASISResult<IEnumerable<IZome>> zomesResult = await result.Result.CelestialBody.LoadZomesAsync();

        //        bool finished = false;
        //        if (zomesResult != null && !zomesResult.IsError && zomesResult.Result != null)
        //        {
        //            if (zomesResult.Result.Count() > 0)
        //            {
        //                CLIEngine.ShowSuccessMessage("Zomes & Holons Loaded Successfully.");
        //                Console.WriteLine("");
        //                ShowZomesAndHolons(zomesResult.Result);
        //            }
        //            else
        //                CLIEngine.ShowSuccessMessage("No Zomes Found.");
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"An Error Occured Loading Zomes/Holons. Reason: {zomesResult.Message}");


        //        //Set some custom properties (will save and load again below to check they persist).
        //        //TODO: Eventually you will be able to set these in the meta data when creating the celestial body.


        //        _superWorld.Age = 777777777777;
        //        _superWorld.Colour = Color.Blue;
        //        _superWorld.CurrentOrbitAngleOfParentStar = 44;
        //        _superWorld.Density = 44;
        //        _superWorld.DimensionLevel = DimensionLevel.Fourth;
        //        _superWorld.SubDimensionLevel = SubDimensionLevel.Second;
        //        _superWorld.DistanceFromParentStarInMetres = 77777777777777;
        //        _superWorld.EclipticLatitute = 33;
        //        _superWorld.EclipticLongitute = 44;
        //        _superWorld.EquatorialLatitute = 11;
        //        _superWorld.EquatorialLongitute = 22;
        //        _superWorld.GalacticLatitute = 23323232;
        //        _superWorld.GalacticLongitute = 43434323;
        //        _superWorld.GravitaionalPull = 7777;
        //        _superWorld.HorizontalLatitute = 452323;
        //        _superWorld.HorizontalLongitute = 4343422;
        //        _superWorld.Mass = 4343232323;
        //        _superWorld.NumberActiveAvatars = 77;
        //        _superWorld.NumberRegisteredAvatars = 444;
        //        _superWorld.Radius = 8888;
        //        _superWorld.OrbitPositionFromParentStar = 77;
        //        _superWorld.RotationPeriod = 878232;
        //        _superWorld.RotationSpeed = 77777;
        //        _superWorld.Size = 88888888;
        //        _superWorld.SpaceQuadrant = SpaceQuadrantType.Gamma;
        //        _superWorld.SpaceSector = 4;
        //        _superWorld.SuperGalacticLatitute = 7777;
        //        _superWorld.SuperGalacticLongitute = 7777;
        //        _superWorld.Temperature = 77;
        //        _superWorld.TiltAngle = 45;
        //        _superWorld.Weight = 77;

        //        //Example of adding a holon to Our World using AddHolonAsync
        //        CLIEngine.ShowWorkingMessage("Saving Test Holon To Our World...");
        //        OASISResult<Holon> ourWorldHolonResult = await _superWorld.AddHolonAsync(new Holon() { Name = "Our World Test Holon" }, STAR.LoggedInAvatar.Id);

        //        if (ourWorldHolonResult != null && !ourWorldHolonResult.IsError && ourWorldHolonResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon Saved Successfully.");
        //            ShowHolonProperties(ourWorldHolonResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Holon. Reason: {ourWorldHolonResult.Message}");


        //        //Example of adding a holon to a new zome using AddHolonAsync
        //        Zome zome = new Zome() { Name = "Our World Test Zome" };
        //        zome.Children.Add(new Holon() { Name = "Our World Test Zome Sub-Holon" });

        //        CLIEngine.ShowWorkingMessage("Saving Test Sub-Holon To Zome...");
        //        ourWorldHolonResult = await zome.AddHolonAsync(new Holon() { Name = "Our World Test Sub-Holon " }, STAR.LoggedInAvatar.Id);

        //        if (ourWorldHolonResult != null && !ourWorldHolonResult.IsError && ourWorldHolonResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Sub-Holon Saved Successfully.");
        //            ShowHolonProperties(ourWorldHolonResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Sub-Holon. Reason: {ourWorldHolonResult.Message}");


        //        //And then saving that new Zome to Our World using AddZomeAsync
        //        CLIEngine.ShowWorkingMessage("Saving Test Zome To Our World...");
        //        OASISResult<IZome> ourWorldZomeResult = await _superWorld.CelestialBodyCore.AddZomeAsync(zome);

        //        if (ourWorldZomeResult != null && !ourWorldZomeResult.IsError && ourWorldZomeResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Zome Saved Successfully.");
        //            ShowHolonProperties(ourWorldZomeResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Zome. Reason: {ourWorldZomeResult.Message}");


        //        //Example of adding zomes/holons in-memory and then saving all in one batch/atomic operation.
        //        CLIEngine.ShowWorkingMessage("Saving Test Zome 2 To Our World...");
        //        zome = new Zome() { Name = "Our World Test Zome 2" };
        //        zome.Children.Add(new Holon() { Name = "Our World Test Zome 2 Sub-Holon 2" });

        //        _superWorld.CelestialBodyCore.Zomes.Add(zome);

        //        CLIEngine.ShowWorkingMessage("Saving Our World...");
        //        OASISResult<ICelestialBody> ourWorldResult = await _superWorld.SaveAsync(); //Will also save the custom properties we set earlier (above).

        //        if (ourWorldResult != null && !ourWorldResult.IsError && ourWorldResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Zome 2 Saved Successfully.");
        //            ShowHolonProperties(ourWorldResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Zome 2. Reason: {ourWorldZomeResult.Message}");


        //        //Re-load the Our World planet to show the new zomes/holons added.
        //        await LoadCelestialBodyAsync(_superWorld, "Our World Planet");
        //        await LoadHolonAsync(_superWorld.Id, "Our World Planet");


        //        CLIEngine.ShowWorkingMessage("Saving Generic Test Zome...");
        //        zome = new Zome() { Name = "Generic Test Zome 2" };
        //        OASISResult<IZome> zomeResult = await zome.SaveAsync();

        //        if (zomeResult != null && !zomeResult.IsError && zomeResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Zome 2 Saved Successfully.");
        //            ShowHolonProperties(zomeResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Zome 2. Reason: {zomeResult.Message}");



        //        //Example saving using Save on the Our World Core GlobalHolonData (shortcut to the Data API below), shows how ALL holons are connected through the cores...
        //        Holon newHolon = new Holon();
        //        newHolon.Name = "Test Data";
        //        newHolon.Description = "Test Desc";
        //        newHolon.HolonType = HolonType.Park;

        //        CLIEngine.ShowWorkingMessage("Saving Generic Test Holon...");
        //        OASISResult<IHolon> holonResult = await result.Result.CelestialBody.CelestialBodyCore.GlobalHolonData.SaveHolonAsync(newHolon);

        //        if (!holonResult.IsError && holonResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon Saved Successfully.");
        //            ShowHolonProperties(holonResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Holon. Reason: {holonResult.Message}");


        //        CLIEngine.ShowWorkingMessage("Loading Generic Test Holon...");
        //        OASISResult<IHolon> holonLoadResult = await newHolon.LoadAsync();

        //        if (!holonLoadResult.IsError && holonLoadResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon Loaded Successfully.");
        //            ShowHolonProperties(holonLoadResult.Result);
        //            //ShowHolonProperties(newHolon); //Can use either this line or the one above.
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Loading Test Holon. Reason: {holonLoadResult.Message}");

        //        newHolon = new Holon();
        //        newHolon.Name = "Test Data2";
        //        newHolon.Description = "Test Desc2";
        //        newHolon.HolonType = HolonType.Restaurant;


        //        //Example saving using Save direct on holon
        //        CLIEngine.ShowWorkingMessage("Saving Generic Test Holon 2...");
        //        holonResult = await newHolon.SaveAsync();

        //        if (!holonResult.IsError && holonResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon 2 Saved Successfully.");
        //            ShowHolonProperties(holonResult.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Holon 2. Reason: {holonResult.Message}");


        //        CLIEngine.ShowWorkingMessage("Loading Generic Test Holon 2...");
        //        holonLoadResult = await newHolon.LoadAsync();

        //        if (!holonLoadResult.IsError && holonLoadResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon 2 Loaded Successfully.");
        //            //ShowHolonProperties(holonLoadResult.Result); //Can use either this line or the one below.
        //            ShowHolonProperties(newHolon);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Loading Test Holon 2. Reason: {holonLoadResult.Message}");


        //        //Example saving using the Data API
        //        newHolon = new Holon();
        //        newHolon.Name = "Test Data3";
        //        newHolon.Description = "Test Desc3";
        //        newHolon.HolonType = HolonType.BusStation;

        //        CLIEngine.ShowWorkingMessage("Saving Generic Test Holon 3...");
        //        OASISResult<IHolon> holonResult2 = await STAR.OASISAPI.Data.SaveHolonAsync(newHolon, STAR.LoggedInAvatar.Id);

        //        if (!holonResult2.IsError && holonResult2.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon 3 Saved Successfully.");
        //            ShowHolonProperties(holonResult2.Result);
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Saving Test Holon 3. Reason: {holonResult2.Message}");


        //        CLIEngine.ShowWorkingMessage("Loading Generic Test Holon 3...");
        //        holonLoadResult = await newHolon.LoadAsync();

        //        if (!holonLoadResult.IsError && holonLoadResult.Result != null)
        //        {
        //            CLIEngine.ShowSuccessMessage("Test Holon 3 Loaded Successfully.");
        //            ShowHolonProperties(holonLoadResult.Result);
        //            //ShowHolonProperties(newHolon); //Can use either this line or the one above.
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage($"Error Loading Test Holon 3. Reason: {holonLoadResult.Message}");


        //        //await RunOASISAPTests(newHolon);


        //        // Build
        //        CoronalEjection ejection = result.Result.CelestialBody.Flare();
        //        //OR
        //        //CoronalEjection ejection = Star.Flare(ourWorld);

        //        // Activate & Launch - Launch & activate the planet (OApp) by shining the 's light upon it...
        //        STAR.Shine(result.Result.CelestialBody);
        //        result.Result.CelestialBody.Shine();

        //        // Deactivate the planet (OApp)
        //        STAR.Dim(result.Result.CelestialBody);

        //        // Deploy the planet (OApp)
        //        STAR.Seed(result.Result.CelestialBody);

        //        // Run Tests
        //        STAR.Twinkle(result.Result.CelestialBody);

        //        // Highlight the Planet (OApp) in the OApp Store (StarNET). *Admin Only*
        //        STAR.Radiate(result.Result.CelestialBody);

        //        // Show how much light the planet (OApp) is emitting into the solar system (StarNET/HoloNET)
        //        STAR.Emit(result.Result.CelestialBody);

        //        // Show stats of the Planet (OApp).
        //        STAR.Reflect(result.Result.CelestialBody);

        //        // Upgrade/update a Planet (OApp).
        //        STAR.Evolve(result.Result.CelestialBody);

        //        // Import/Export hApp, dApp & others.
        //        STAR.Mutate(result.Result.CelestialBody);

        //        // Send/Receive Love
        //        STAR.Love(result.Result.CelestialBody);

        //        // Show network stats/management/settings
        //        STAR.Burst(result.Result.CelestialBody);

        //        // Reserved For Future Use...
        //        STAR.Super(result.Result.CelestialBody);

        //        // Delete a planet (OApp).
        //        STAR.Dust(result.Result.CelestialBody);
        //    }
        //}

        //private static async Task LoadCelestialBodyAsync(ICelestialBody celestialBody, string name)
        //{
        //    CLIEngine.ShowWorkingMessage($"Loading {name}...");
        //    OASISResult<ICelestialBody> celestialBodyResult = await celestialBody.LoadAsync();

        //    //switch (celestialBody.HolonType)
        //    //{
        //    //    case HolonType.Moon:
        //    //        OASISResult<Moon> worldResult = await celestialBody.LoadAsync<Moon>();
        //    //        break;
        //    //}

        //    if (celestialBodyResult != null && !celestialBodyResult.IsError && celestialBodyResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage($"{name} Loaded Successfully.");
        //        ShowHolonProperties(celestialBodyResult.Result);
        //        Console.WriteLine("");
        //        ShowZomesAndHolons(celestialBodyResult.Result.CelestialBodyCore.Zomes, $"{name} Contains {celestialBodyResult.Result.CelestialBodyCore.Zomes.Count()} Zome(s): ");
        //    }
        //}

        //private static async Task LoadCelestialBodyAsync<T>(T celestialBody, string name) where T : ICelestialBody, new()
        //{
        //    CLIEngine.ShowWorkingMessage($"Loading {name}...");
        //    OASISResult<T> celestialBodyResult = await celestialBody.LoadAsync<T>();

        //    if (celestialBodyResult != null && !celestialBodyResult.IsError && celestialBodyResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage($"{name} Loaded Successfully.");
        //        ShowHolonProperties(celestialBodyResult.Result);
        //        Console.WriteLine("");
        //        ShowZomesAndHolons(celestialBodyResult.Result.CelestialBodyCore.Zomes, string.Concat(" ", name, " Contains ", celestialBodyResult.Result.CelestialBodyCore.Zomes.Count(), " Zome(s)", celestialBodyResult.Result.CelestialBodyCore.Zomes.Count > 0 ? ":" : ""));
        //    }
        //}

        //private static async Task LoadHolonAsync(Guid id, string name)
        //{
        //    CLIEngine.ShowWorkingMessage($"Loading Holon {name}...");
        //    OASISResult<IHolon> holonResult = await STAR.OASISAPI.Data.LoadHolonAsync(id);

        //    if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage($"{name} Loaded Successfully.");
        //        ShowHolonProperties(holonResult.Result);
        //        //Console.WriteLine("");

        //        //if (holonResult.Result.Children != null && holonResult.Result.Children.Count > 0)
        //        //    ShowHolons(holonResult.Result.Children);
        //    }
        //}

        //private static void ShowHolonProperties(IHolon holon, bool showChildren = true)
        //{
        //    Console.WriteLine("");
        //    Console.WriteLine(string.Concat(" Id: ", holon.Id));
        //    Console.WriteLine(string.Concat(" Holon Type: ", Enum.GetName(typeof(HolonType), holon.HolonType)));
        //    Console.WriteLine(string.Concat(" Created By Avatar Id: ", holon.CreatedByAvatarId));
        //    Console.WriteLine(string.Concat(" Created Date: ", holon.CreatedDate));
        //    Console.WriteLine(string.Concat(" Modifed By Avatar Id: ", holon.ModifiedByAvatarId));
        //    Console.WriteLine(string.Concat(" Modifed Date: ", holon.ModifiedDate));
        //    Console.WriteLine(string.Concat(" Name: ", holon.Name));
        //    Console.WriteLine(string.Concat(" Description: ", holon.Description));
        //    Console.WriteLine(string.Concat(" Created OASIS Type: ", holon.CreatedOASISType != null ? holon.CreatedOASISType.Name : ""));
        //    Console.WriteLine(string.Concat(" Created On Provider Type: ", holon.CreatedProviderType != null ? holon.CreatedProviderType.Name : ""));
        //    Console.WriteLine(string.Concat(" Instance Saved On Provider Type: ", holon.InstanceSavedOnProviderType != null ? holon.InstanceSavedOnProviderType.Name : ""));
        //    Console.WriteLine(string.Concat(" Active: ", holon.IsActive ? "True" : "False"));
        //    Console.WriteLine(string.Concat(" Version: ", holon.Version));
        //    Console.WriteLine(string.Concat(" Version Id: ", holon.VersionId));
        //    Console.WriteLine(string.Concat(" Custom Key: ", holon.CustomKey));
        //    Console.WriteLine(string.Concat(" Dimension Level: ", Enum.GetName(typeof(DimensionLevel), holon.DimensionLevel)));
        //    Console.WriteLine(string.Concat(" Sub-Dimension Level: ", Enum.GetName(typeof(SubDimensionLevel), holon.SubDimensionLevel)));

        //    ICelestialHolon celestialHolon = holon as ICelestialHolon;

        //    if (celestialHolon != null)
        //    {
        //        Console.WriteLine(string.Concat(" Age: ", celestialHolon.Age));
        //        Console.WriteLine(string.Concat(" Colour: ", celestialHolon.Colour));
        //        Console.WriteLine(string.Concat(" Ecliptic Latitute: ", celestialHolon.EclipticLatitute));
        //        Console.WriteLine(string.Concat(" Ecliptic Longitute: ", celestialHolon.EclipticLongitute));
        //        Console.WriteLine(string.Concat(" Equatorial Latitute: ", celestialHolon.EquatorialLatitute));
        //        Console.WriteLine(string.Concat(" Equatorial Longitute: ", celestialHolon.EquatorialLongitute));
        //        Console.WriteLine(string.Concat(" Galactic Latitute: ", celestialHolon.GalacticLatitute));
        //        Console.WriteLine(string.Concat(" Galactic Longitute: ", celestialHolon.GalacticLongitute));
        //        Console.WriteLine(string.Concat(" Horizontal Latitute: ", celestialHolon.HorizontalLatitute));
        //        Console.WriteLine(string.Concat(" Horizontal Longitute: ", celestialHolon.HorizontalLongitute));
        //        Console.WriteLine(string.Concat(" Radius: ", celestialHolon.Radius));
        //        Console.WriteLine(string.Concat(" Size: ", celestialHolon.Size));
        //        Console.WriteLine(string.Concat(" Space Quadrant: ", Enum.GetName(typeof(SpaceQuadrantType), celestialHolon.SpaceQuadrant)));
        //        Console.WriteLine(string.Concat(" Space Sector: ", celestialHolon.SpaceSector));
        //        Console.WriteLine(string.Concat(" Super Galactic Latitute: ", celestialHolon.SuperGalacticLatitute));
        //        Console.WriteLine(string.Concat(" Super Galactic Longitute: ", celestialHolon.SuperGalacticLongitute));
        //        Console.WriteLine(string.Concat(" Temperature: ", celestialHolon.Temperature));
        //    }

        //    ICelestialBody celestialBody = holon as ICelestialBody;

        //    if (celestialBody != null)
        //    {
        //        Console.WriteLine(string.Concat(" Current Orbit Angle Of Parent Star: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Density: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Distance From Parent Star In Metres: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Gravitaional Pull: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Mass: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Number Active Avatars: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Number Registered Avatars: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Orbit Position From Parent Star: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Rotation Period: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Rotation Speed: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Tilt Angle: ", celestialBody.Age));
        //        Console.WriteLine(string.Concat(" Weight: ", celestialBody.Age));
        //    }

        //    if (holon.ParentHolon != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Holon");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Holon Id: ", holon.ParentHolonId));


        //    if (holon.ParentCelestialBody != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Celestial Body");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Celestial Body Id: ", holon.ParentCelestialBodyId));


        //    if (holon.ParentCelestialSpace != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Celestial Space");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Celestial Space Id: ", holon.ParentCelestialSpaceId));


        //    if (holon.ParentGreatGrandSuperStar != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Great Grand Super Star");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Great Grand Super Star Id: ", holon.ParentGreatGrandSuperStarId));


        //    if (holon.ParentGrandSuperStar != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Grand Super Star");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Grand Super Star Id: ", holon.ParentGrandSuperStarId));


        //    if (holon.ParentSuperStar != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Super Star");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Super Star Id: ", holon.ParentSuperStarId));


        //    if (holon.ParentStar != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Star");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Star Id: ", holon.ParentStarId));


        //    if (holon.ParentPlanet != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Planet");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Planet Id: ", holon.ParentPlanetId));


        //    if (holon.ParentMoon != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Moon");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Moon Id: ", holon.ParentMoonId));

        //    if (holon.ParentOmniverse != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Omniverse");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Omniverse Id: ", holon.ParentOmniverseId));


        //    if (holon.ParentMultiverse != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Multiverse");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Multiverse Id: ", holon.ParentMultiverseId));


        //    if (holon.ParentDimension != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Dimension");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Dimension Id: ", holon.ParentDimensionId));


        //    if (holon.ParentUniverse != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Universe");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Universe Id: ", holon.ParentUniverseId));


        //    if (holon.ParentGalaxyCluster != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Galaxy Cluster");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Galaxy Cluster Id: ", holon.ParentGalaxyClusterId));


        //    if (holon.ParentGalaxy != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Galaxy");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Galaxy Id: ", holon.ParentGalaxyId));


        //    if (holon.ParentSolarSystem != null)
        //        ShowHolonBasicProperties(holon.ParentHolon, "Parent Solar System");
        //    else
        //        Console.WriteLine(string.Concat(" Parent Solar System Id: ", holon.ParentSolarSystemId));


        //    Console.WriteLine(string.Concat(" Children: ", holon.Children.Count));
        //    Console.WriteLine(string.Concat(" All Children: ", holon.AllChildren.Count));

        //    if (showChildren)
        //    {
        //        ShowHolons(holon.Children);
        //        //Console.WriteLine("");
        //    }

        //    if (holon.MetaData != null && holon.MetaData.Keys.Count > 0)
        //    {
        //        Console.WriteLine(string.Concat(" Meta Data: ", holon.MetaData.Keys.Count, " Key(s) Found:"));
        //        foreach (string key in holon.MetaData.Keys)
        //            Console.WriteLine(string.Concat("   ", key, " = ", holon.MetaData[key]));
        //    }
        //    else
        //        Console.WriteLine(string.Concat(" Meta Data: None"));

        //    if (holon.ProviderMetaData != null && holon.ProviderMetaData.Keys.Count > 0)
        //    {
        //        Console.WriteLine(string.Concat(" Provider Meta Data: "));

        //        foreach (ProviderType providerType in holon.ProviderMetaData.Keys)
        //        {
        //            Console.WriteLine(string.Concat(" Provider: ", Enum.GetName(typeof(ProviderType), providerType)));

        //            foreach (string key in holon.ProviderMetaData[providerType].Keys)
        //                Console.WriteLine(string.Concat("Key: ", key, "Value: ", holon.ProviderMetaData[providerType][key]));
        //        }
        //    }
        //    else
        //        Console.WriteLine(string.Concat(" Provider Meta Data: None"));

        //    Console.WriteLine("");
        //    Console.WriteLine(string.Concat(" Provider Unique Storage Keys: "));

        //    foreach (ProviderType providerType in holon.ProviderUniqueStorageKey.Keys)
        //        Console.WriteLine(string.Concat("   Provider: ", Enum.GetName(typeof(ProviderType), providerType), " = ", holon.ProviderUniqueStorageKey[providerType]));

        //}

        ///// <summary>
        ///// This is a good example of how to programatically interact with the OASIS API including scripting etc...
        ///// </summary>
        ///// <returns></returns>
        //private static async Task RunOASISAPTests()
        //{
        //    // BEGIN OASIS API DEMO ***********************************************************************************
        //    CLIEngine.ShowWorkingMessage("BEGINNING OASIS API TEST'S...");

        //    CLIEngine.ShowWorkingMessage("Beginning Wallet/Key API Tests...");

        //    CLIEngine.ShowWorkingMessage("Linking Public Key to Solana Wallet...");
        //    OASISResult<Guid> keyLinkResult = STAR.OASISAPI.Keys.LinkProviderPublicKeyToAvatarByEmail(Guid.Empty, "davidellams@hotmail.com", ProviderType.SolanaOASIS, "TEST PUBLIC KEY");

        //    if (!keyLinkResult.IsError && keyLinkResult.Result != Guid.Empty)
        //        CLIEngine.ShowSuccessMessage($"Successfully linked public key to Solana Wallet. WalletID: {keyLinkResult.Result}");
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured linking key. Reason: {keyLinkResult.Message}");


        //    CLIEngine.ShowWorkingMessage("Linking Private Key to Solana Wallet...");
        //    keyLinkResult = STAR.OASISAPI.Keys.LinkProviderPrivateKeyToAvatarByEmail(keyLinkResult.Result, "davidellams@hotmail.com", ProviderType.SolanaOASIS, "TEST PRIVATE KEY");

        //    if (!keyLinkResult.IsError && keyLinkResult.Result != Guid.Empty)
        //        CLIEngine.ShowSuccessMessage($"Successfully linked private key to Solana Wallet. WalletID: {keyLinkResult.Result}");
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured linking key. Reason: {keyLinkResult.Message}");


        //    CLIEngine.ShowWorkingMessage("Generating KeyPair & Linking to EOS Wallet...");
        //    OASISResult<KeyPair> generateKeyPairResult = STAR.OASISAPI.Keys.GenerateKeyPairAndLinkProviderKeysToAvatarByEmail("davidellams@hotmail.com", ProviderType.EOSIOOASIS, true, true);

        //    if (!generateKeyPairResult.IsError && generateKeyPairResult.Result != null)
        //        CLIEngine.ShowSuccessMessage($"Successfully generated new keypair and linked to EOS Wallet. Public Key: {generateKeyPairResult.Result.PublicKey}, Private Key: {generateKeyPairResult.Result.PrivateKey}");
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured generating keypair. Reason: {generateKeyPairResult.Message}");

        //    CLIEngine.ShowWorkingMessage("Getting all Provider Public Keys For Avatar...");
        //    OASISResult<Dictionary<ProviderType, List<string>>> keysResult = STAR.OASISAPI.Keys.GetAllProviderPublicKeysForAvatarByEmail("davidellams@hotmail.com");

        //    if (!keysResult.IsError && keysResult.Result != null)
        //    {
        //        string message = "";
        //        foreach (ProviderType providerType in keysResult.Result.Keys)
        //        {
        //            foreach (string key in keysResult.Result[providerType])
        //                message = string.Concat(message, providerType.ToString(), ": ", key, "\n");
        //        }

        //        CLIEngine.ShowSuccessMessage($"Successfully retreived keys:\n{message}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured getting keys. Reason: {keysResult.Message}");


        //    CLIEngine.ShowWorkingMessage("Getting all Provider Private Keys For Avatar...");
        //    keysResult = STAR.OASISAPI.Keys.GetAllProviderPrivateKeysForAvatarByUsername("davidellams@hotmail.com");

        //    if (!keysResult.IsError && keysResult.Result != null)
        //    {
        //        string message = "";
        //        foreach (ProviderType providerType in keysResult.Result.Keys)
        //        {
        //            foreach (string key in keysResult.Result[providerType])
        //                message = string.Concat(message, providerType.ToString(), ": ", key, "\n");
        //        }

        //        CLIEngine.ShowSuccessMessage($"Successfully retreived keys\n{message}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured getting keys. Reason: {keysResult.Message}");


        //    CLIEngine.ShowWorkingMessage("Getting all Provider Unique Storage Keys For Avatar...");
        //    OASISResult<Dictionary<ProviderType, string>> uniqueKeysResult = STAR.OASISAPI.Keys.GetAllProviderUniqueStorageKeysForAvatarByEmail("davidellams@hotmail.com");

        //    if (!uniqueKeysResult.IsError && uniqueKeysResult.Result != null)
        //    {
        //        string message = "";
        //        foreach (ProviderType providerType in uniqueKeysResult.Result.Keys)
        //            message = string.Concat(message, providerType.ToString(), ": ", uniqueKeysResult.Result[providerType], "\n");

        //        CLIEngine.ShowSuccessMessage($"Successfully retreived keys:\n{message}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured getting keys. Reason: {uniqueKeysResult.Message}");


        //    CLIEngine.ShowSuccessMessage("Wallet/Key API Tests Complete.");

        //   // Console.WriteLine("Press Any Key To Continue...");
        //   // Console.ReadKey();

        //    //Set auto-replicate for all providers except IPFS and Neo4j.
        //    //EnableOrDisableAutoProviderList(ProviderManager.Instance.SetAutoReplicateForAllProviders, true, null, "Enabling Auto-Replication For All Providers...", "Auto-Replication Successfully Enabled For All Providers.", "Error Occured Enabling Auto-Replication For All Providers.");
        //    CLIEngine.ShowWorkingMessage("Enabling Auto-Replication For All Providers...");
        //    bool isSuccess = ProviderManager.Instance.SetAutoReplicateForAllProviders(true);
        //    HandleBooleansResponse(isSuccess, "Auto-Replication Successfully Enabled For All Providers.", "Error Occured Enabling Auto-Replication For All Providers.");

        //    CLIEngine.ShowWorkingMessage("Disabling Auto-Replication For IPFSOASIS & Neo4jOASIS Providers...");
        //    isSuccess = ProviderManager.Instance.SetAutoReplicationForProviders(false, new List<ProviderType>() { ProviderType.IPFSOASIS, ProviderType.Neo4jOASIS });
        //    //EnableOrDisableAutoProviderList(ProviderManager.Instance.SetAutoReplicationForProviders, false, new List<ProviderType>() { ProviderType.IPFSOASIS, ProviderType.Neo4jOASIS }, "Enabling Auto-Replication For All Providers...", "Auto-Replication Successfully Enabled For All Providers.", "Error Occured Enabling Auto-Replication For All Providers.");
        //    HandleBooleansResponse(isSuccess, "Auto-Replication Successfully Disabled For IPFSOASIS & Neo4jOASIS Providers.", "Error Occured Disabling Auto-Replication For IPFSOASIS & Neo4jOASIS Providers.");

        //    //Set auto-failover for all providers except Holochain.
        //    CLIEngine.ShowWorkingMessage("Enabling Auto-FailOver For All Providers...");
        //    isSuccess = ProviderManager.Instance.SetAutoFailOverForAllProviders(true);
        //    HandleBooleansResponse(isSuccess, "Auto-FailOver Successfully Enabled For All Providers.", "Error Occured Enabling Auto-FailOver For All Providers.");

        //    CLIEngine.ShowWorkingMessage("Disabling Auto-FailOver For HoloOASIS Provider...");
        //    isSuccess = ProviderManager.Instance.SetAutoFailOverForProviders(false, new List<ProviderType>() { ProviderType.HoloOASIS });
        //    HandleBooleansResponse(isSuccess, "Auto-FailOver Successfully Disabled For HoloOASIS.", "Error Occured Disabling Auto-FailOver For HoloOASIS Provider.");

        //    //Set auto-load balance for all providers except Ethereum.
        //    CLIEngine.ShowWorkingMessage("Enabling Auto-Load-Balancing For All Providers...");
        //    isSuccess = ProviderManager.Instance.SetAutoLoadBalanceForAllProviders(true);
        //    HandleBooleansResponse(isSuccess, "Auto-FailOver Successfully Disabled For HoloOASIS.", "Error Occured Disabling Auto-FailOver For HoloOASIS Provider.");

        //    CLIEngine.ShowWorkingMessage("Disabling Auto-Load-Balancing For EthereumOASIS Provider...");
        //    isSuccess = ProviderManager.Instance.SetAutoLoadBalanceForProviders(false, new List<ProviderType>() { ProviderType.EthereumOASIS });
        //    HandleBooleansResponse(isSuccess, "Auto-Load-Balancing Successfully Disabled For EthereumOASIS.", "Error Occured Disabling Auto-Load-Balancing For EthereumOASIS Provider.");

        //    // Set the default provider to MongoDB.
        //    // Set last param to false if you wish only the next call to use this provider.
        //    CLIEngine.ShowWorkingMessage("Setting Default Provider to MongoDBOASIS...");
        //    //HandleOASISResponse(ProviderManager.Instance.SetAndActivateCurrentStorageProvider(ProviderType.MongoDBOASIS, true), "Successfully Set Default Provider To MongoDBOASIS Provider.", "Error Occured Setting Default Provider To MongoDBOASIS.");
        //    HandleOASISResponse(await ProviderManager.Instance.SetAndActivateCurrentStorageProviderAsync(ProviderType.MongoDBOASIS, true), "Successfully Set Default Provider To MongoDBOASIS Provider.", "Error Occured Setting Default Provider To MongoDBOASIS.");

        //    //  Give HoloOASIS Store permission for the Name field(the field will only be stored on Holochain).
        //    CLIEngine.ShowWorkingMessage("Granting HoloOASIS Provider Store Permission For The Name Field...");
        //    STAR.OASISAPI.Avatar.Config.FieldToProviderMappings.Name.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.Store, Provider = ProviderType.HoloOASIS });
        //    CLIEngine.ShowSuccessMessage("Permission Granted.");

        //    // Give all providers read/write access to the Karma field (will allow them to read and write to the field but it will only be stored on Holochain).
        //    // You could choose to store it on more than one provider if you wanted the extra redundancy (but not normally needed since Holochain has a lot of redundancy built in).
        //    CLIEngine.ShowWorkingMessage("Granting All Providers Read/Write Permission For The Karma Field...");
        //    STAR.OASISAPI.Avatar.Config.FieldToProviderMappings.Karma.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.ReadWrite, Provider = ProviderType.All });
        //    CLIEngine.ShowSuccessMessage("Permission Granted.");

        //    //Give Ethereum read-only access to the DOB field.
        //    CLIEngine.ShowWorkingMessage("Granting EthereumOASIS Providers Read-Only Permission For The DOB Field...");
        //    STAR.OASISAPI.Avatar.Config.FieldToProviderMappings.DOB.Add(new ProviderManagerConfig.FieldToProviderMappingAccess { Access = ProviderManagerConfig.ProviderAccess.ReadOnly, Provider = ProviderType.EthereumOASIS });
        //    CLIEngine.ShowSuccessMessage("Permission Granted.");

        //    // All calls are load-balanced and have multiple redudancy/fail over for all supported OASIS Providers.
        //    CLIEngine.ShowWorkingMessage("Loading All Avatars Load Balanced Across All Providers...");
        //    OASISResult<IEnumerable<IAvatar>> avatarsResult = STAR.OASISAPI.Avatar.LoadAllAvatars(); // Load-balanced across all providers.

        //    if (!avatarsResult.IsError && avatarsResult.Result != null)
        //        CLIEngine.ShowSuccessMessage($"{avatarsResult.Result.Count()} Avatars Loaded.");
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured loading avatars. Reason: {avatarsResult.Message}");

        //    CLIEngine.ShowWorkingMessage("Loading All Avatars Only For The MongoDBOASIS Provider...");
        //    avatarsResult = STAR.OASISAPI.Avatar.LoadAllAvatars(false, true, true, ProviderType.MongoDBOASIS); // Only loads from MongoDB.

        //    if (!avatarsResult.IsError && avatarsResult.Result != null)
        //        CLIEngine.ShowSuccessMessage($"{avatarsResult.Result.Count()} Avatars Loaded.");
        //    else
        //        CLIEngine.ShowErrorMessage($"Error occured loading avatars. Reason: {avatarsResult.Message}");

        //    CLIEngine.ShowWorkingMessage("Loading Avatar Only For The HoloOASIS Provider...");
        //    OASISResult<IAvatar> avatarResult = STAR.OASISAPI.Avatar.LoadAvatar(STAR.LoggedInAvatar.Id, false, true, ProviderType.HoloOASIS); // Only loads from Holochain.

        //    if (!avatarResult.IsError && avatarResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage("Avatar Loaded Successfully");
        //        CLIEngine.ShowSuccessMessage($"Avatar ID: {avatarResult.Result.Id}");
        //        CLIEngine.ShowSuccessMessage($"Avatar Name: {avatarResult.Result.FullName}");
        //        CLIEngine.ShowSuccessMessage($"Avatar Created Date: {avatarResult.Result.CreatedDate}");
        //        CLIEngine.ShowSuccessMessage($"Avatar Last Beamed In Date: {avatarResult.Result.LastBeamedIn}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage("Error Loading Avatar.");


        //    Holon newHolon = new Holon();
        //    newHolon.Name = "Test Data";
        //    newHolon.Description = "Test Desc";
        //    newHolon.HolonType = HolonType.Park;

        //    CLIEngine.ShowWorkingMessage("Saving Test Holon (Load Balanced Across All Providers)...");
        //    HandleOASISResponse(STAR.OASISAPI.Data.SaveHolon(newHolon, STAR.LoggedInAvatar.Id), "Holon Saved Successfully.", "Error Saving Holon."); // Load-balanced across all providers.

        //    CLIEngine.ShowWorkingMessage("Saving Test Holon Only For The EthereumOASIS Provider...");
        //    HandleOASISResponse(STAR.OASISAPI.Data.SaveHolon(newHolon, STAR.LoggedInAvatar.Id, true, true, 0, true, false, ProviderType.EthereumOASIS), "Holon Saved Successfully.", "Error Saving Holon."); //  Only saves to Etherum.


        //    CLIEngine.ShowWorkingMessage("Creating & Drawing Route On Map Between 2 Test Holons (Load Balanced Across All Providers)...");
        //    HandleBooleansResponse(STAR.OASISAPI.Map.CreateAndDrawRouteOnMapBetweenHolons(newHolon, newHolon), "Route Created Successfully.", "Error Creating Route."); // Load-balanced across all providers.

        //    CLIEngine.ShowWorkingMessage("Loading Test Holon (Load Balanced Across All Providers)...");
        //    OASISResult<IHolon> holonResult = STAR.OASISAPI.Data.LoadHolon(newHolon.Id); // Load-balanced across all providers.

        //    if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage("Holon Loaded Successfully.");
        //        CLIEngine.ShowSuccessMessage($"Id: {holonResult.Result.Id}");
        //        CLIEngine.ShowSuccessMessage($"Name: {holonResult.Result.Name}");
        //        CLIEngine.ShowSuccessMessage($"Description: {holonResult.Result.Description}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage("Error Loading Holon");

        //    CLIEngine.ShowWorkingMessage("Loading Test Holon Only For IPFSOASIS Provider...");
        //    holonResult = STAR.OASISAPI.Data.LoadHolon(newHolon.Id, true, true, 0, true, false, HolonType.All, 0, ProviderType.IPFSOASIS); // Only loads from IPFS.

        //    if (holonResult != null && !holonResult.IsError && holonResult.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage("Holon Loaded Successfully.");
        //        CLIEngine.ShowSuccessMessage($"Id: {holonResult.Result.Id}");
        //        CLIEngine.ShowSuccessMessage($"Name: {holonResult.Result.Name}");
        //        CLIEngine.ShowSuccessMessage($"Description: {holonResult.Result.Description}");
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage("Error Loading Holon");

        //    CLIEngine.ShowWorkingMessage("Loading All Holons Of Type Moon Only For HoloOASIS Provider...");
        //    HandleHolonsOASISResponse(STAR.OASISAPI.Data.LoadAllHolons(HolonType.Moon, true, true, 0, true, false, HolonType.All, 0, ProviderType.HoloOASIS)); // Loads all moon (OAPPs) from Holochain.

        //    CLIEngine.ShowWorkingMessage("Loading All Holons From The Current Default Provider (With Auto-FailOver)...");
        //    HandleHolonsOASISResponse(STAR.OASISAPI.Data.LoadAllHolons(HolonType.All, true, true, 0, true, false, HolonType.All, 0, ProviderType.Default)); // Loads all holons from current default provider.

        //    CLIEngine.ShowWorkingMessage("Loading All Park Holons From All Providers (With Auto-Load-Balance & Auto-FailOver)...");
        //    HandleHolonsOASISResponse(STAR.OASISAPI.Data.LoadAllHolons(HolonType.Park, true, true, 0, true, false, HolonType.All, 0, ProviderType.All)); // Loads all parks from all providers (load-balanced/fail over).

        //    //CLIEngine.ShowWorkingMessage("Loading All Park Holons From All Providers (With Auto-Load-Balance & Auto-FailOver)...");
        //    STAR.OASISAPI.Data.LoadAllHolons(HolonType.Park); // shorthand for above.

        //    CLIEngine.ShowWorkingMessage("Loading All Quest Holons From All Providers (With Auto-Load-Balance & Auto-FailOver)...");
        //    HandleHolonsOASISResponse(STAR.OASISAPI.Data.LoadAllHolons(HolonType.Quest)); //  Loads all quests from all providers.

        //    CLIEngine.ShowWorkingMessage("Loading All Restaurant Holons From All Providers (With Auto-Load-Balance & Auto-FailOver)...");
        //    HandleHolonsOASISResponse(STAR.OASISAPI.Data.LoadAllHolons(HolonType.Restaurant)); //  Loads all resaurants from all providers.

        //    // Holochain Support

        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating Holochain Tests...");

        //        if (!STAR.OASISAPI.Providers.Holochain.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating Holochain Provider...");
        //            STAR.OASISAPI.Providers.Holochain.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("Holochain Provider Activated.");
        //        }

        //        CLIEngine.ShowWorkingMessage("Loading Avatar By Email...");
        //        OASISResult<IAvatar> avatarResultHolochain = STAR.OASISAPI.Providers.Holochain.LoadAvatarByEmail("davidellams@hotmail.com");

        //        if (!avatarResultHolochain.IsError && avatarResultHolochain.Result != null)
        //            CLIEngine.ShowSuccessMessage($"Avatar Loaded Successfully. Id: {avatarResultHolochain.Result.Id}");

        //        CLIEngine.ShowWorkingMessage("Calling Test Zome Function on HoloNET Client...");
        //        //await STAR.OASISAPI.Providers.Holochain.HoloNETClient.CallZomeFunctionAsync(STAR.OASISAPI.Providers.Holochain.HoloNETClient.Config.AgentPubKey, "our_world_core", "load_holons", null);
        //        await STAR.OASISAPI.Providers.Holochain.HoloNETClientAppAgent.CallZomeFunctionAsync("our_world_core", "load_holons", null);
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during Holochain Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("Holochain Tests Completed.");

        //    // IPFS Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating IPFS Tests...");

        //        if (!STAR.OASISAPI.Providers.IPFS.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating IPFS Provider...");
        //            STAR.OASISAPI.Providers.IPFS.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("IPFS Provider Activated.");
        //        }

        //        IFileSystemNode result = await STAR.OASISAPI.Providers.IPFS.IPFSClient.FileSystem.AddTextAsync("TEST");
        //        CLIEngine.ShowMessage($"Id of IPFS Write Test: {result.Id}");
        //        CLIEngine.ShowMessage($"Data Writen for IPFS Write Test: {result.DataBytes.Length} bytes");

        //        string ipfsResult = await STAR.OASISAPI.Providers.IPFS.IPFSClient.FileSystem.ReadAllTextAsync(result.Id);
        //        CLIEngine.ShowMessage($"IPFS Read Result: {ipfsResult}");
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during IPFS Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("IPFS Tests Completed.");

        //    // Ethereum Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating Ethereum Tests...");

        //        if (!STAR.OASISAPI.Providers.Ethereum.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating Ethereum Provider...");
        //            STAR.OASISAPI.Providers.Ethereum.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("Ethereum Provider Activated.");
        //        }

        //        await STAR.OASISAPI.Providers.Ethereum.Web3Client.Client.SendRequestAsync(new Nethereum.JsonRpc.Client.RpcRequest("id", "test"));
        //        await STAR.OASISAPI.Providers.Ethereum.Web3Client.Eth.Blocks.GetBlockNumber.SendRequestAsync("");
        //        Contract contract = STAR.OASISAPI.Providers.Ethereum.Web3Client.Eth.GetContract("abi", "contractAddress");
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during Ethereum Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("Ethereum Tests Completed.");

        //    // EOSIO Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating EOSIO Tests...");

        //        if (!STAR.OASISAPI.Providers.EOSIO.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating EOSIO Provider...");
        //            STAR.OASISAPI.Providers.EOSIO.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("EOSIO Provider Activated.");
        //        }

        //        STAR.OASISAPI.Providers.EOSIO.ChainAPI.GetTableRows("accounts", "accounts", "users", "true", 0, 0, 1, 3);
        //        STAR.OASISAPI.Providers.EOSIO.ChainAPI.GetBlock("block");
        //        STAR.OASISAPI.Providers.EOSIO.ChainAPI.GetAccount("test.account");
        //        STAR.OASISAPI.Providers.EOSIO.ChainAPI.GetCurrencyBalance("test.account", "", "");
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during EOSIO Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("EOSIO Tests Completed.");

        //    // Graph DB Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating Neo4j (Graph DB) Tests...");

        //        if (!STAR.OASISAPI.Providers.Neo4j.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating Neo4j Provider...");
        //            STAR.OASISAPI.Providers.Neo4j.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("Neo4j Provider Activated.");
        //        }

        //        CLIEngine.ShowWorkingMessage("Executing Graph Cypher Test...");

        //        var session = STAR.OASISAPI.Providers.Neo4j.Driver.AsyncSession();

        //        await session.ReadTransactionAsync(async transaction =>
        //        {
        //            var cursor = await transaction.RunAsync(@"
        //                    MATCH (av:Avatar)                        
        //                    RETURN av.FirstName AS firstname,av.LastName AS lastname"
        //            );

        //            IEnumerable<IAvatar> objList = await cursor.ToListAsync(record => new Avatar
        //            {
        //                FirstName = record["firstname"].As<string>(),
        //                LastName = record["lastname"].As<string>()
        //            });
        //        });

        //        //await STAR.OASISAPI.Providers.Neo4j.Driver.Cypher.Merge("(a:Avatar { Id: avatar.Id })").OnCreate().Set("a = avatar").ExecuteWithoutResultsAsync(); //Insert/Update Avatar.
        //        //await STAR.OASISAPI.Providers.Neo4j.GraphClient.Cypher.Merge("(a:Avatar { Id: avatar.Id })").OnCreate().Set("a = avatar").ExecuteWithoutResultsAsync(); //Insert/Update Avatar.
        //        //Avatar newAvatar = STAR.OASISAPI.Providers.Neo4j.GraphClient.Cypher.Match("(p:Avatar {Username: {nameParam}})").WithParam("nameParam", "davidellams@hotmail.com").Return(p => p.As<Avatar>()).ResultsAsync.Result.Single(); //Load Avatar.
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during Neo4j Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("Neo4j Tests Completed.");

        //    // Document/Object DB Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating MongoDB Tests...");

        //        if (!STAR.OASISAPI.Providers.MongoDB.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating MongoDB Provider...");
        //            STAR.OASISAPI.Providers.MongoDB.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("MongoDB Provider Activated.");
        //        }

        //        CLIEngine.ShowWorkingMessage("Listing Collction Names...");
        //        STAR.OASISAPI.Providers.MongoDB.Database.MongoDB.ListCollectionNames();

        //        CLIEngine.ShowWorkingMessage("Getting Avatar Collection...");
        //        //IMongoCollection<IAvatar> collection = STAR.OASISAPI.Providers.MongoDB.Database.MongoDB.GetCollection<Avatar>("Avatar");
        //        STAR.OASISAPI.Providers.MongoDB.Database.MongoDB.GetCollection<Avatar>("Avatar");

        //        //if (collection != null)
        //        //    CLIEngine.ShowSuccessMessage($"{collection.Coi} avatars found.");

        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during MongoDB Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("MongoDB Tests Completed.");

        //    // SEEDS Support
        //    try
        //    {
        //        CLIEngine.ShowWorkingMessage("Initiating SEEDS Tests...");

        //        if (!STAR.OASISAPI.Providers.SEEDS.IsProviderActivated)
        //        {
        //            CLIEngine.ShowWorkingMessage("Activating SEEDS Provider...");
        //            STAR.OASISAPI.Providers.SEEDS.ActivateProvider();
        //            CLIEngine.ShowSuccessMessage("SEEDS Provider Activated.");
        //        }

        //        CLIEngine.ShowWorkingMessage("Getting Balance for account davidsellams...");
        //        string balance = STAR.OASISAPI.Providers.SEEDS.GetBalanceForTelosAccount("davidsellams");
        //        CLIEngine.ShowSuccessMessage(string.Concat("Balance: ", balance));

        //        CLIEngine.ShowWorkingMessage("Getting Balance for account nextgenworld...");
        //        balance = STAR.OASISAPI.Providers.SEEDS.GetBalanceForTelosAccount("nextgenworld");
        //        CLIEngine.ShowSuccessMessage(string.Concat("Balance: ", balance));

        //        CLIEngine.ShowWorkingMessage("Getting Account for account davidsellams...");
        //        GetAccountResponseDto account = STAR.OASISAPI.Providers.SEEDS.TelosOASIS.GetTelosAccount("davidsellams");

        //        if (account != null)
        //        {
        //            CLIEngine.ShowSuccessMessage(string.Concat("Account.account_name: ", account.AccountName));
        //            CLIEngine.ShowSuccessMessage(string.Concat("Account.created: ", account.Created.ToString()));
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage("Account not found.");

        //        CLIEngine.ShowWorkingMessage("Getting Account for account nextgenworld...");
        //        account = STAR.OASISAPI.Providers.SEEDS.TelosOASIS.GetTelosAccount("nextgenworld");

        //        if (account != null)
        //        {
        //            CLIEngine.ShowSuccessMessage(string.Concat("Account.account_name: ", account.AccountName));
        //            CLIEngine.ShowSuccessMessage(string.Concat("Account.created: ", account.Created.ToString()));
        //        }
        //        else
        //            CLIEngine.ShowErrorMessage("Account not found.");

        //        // Check that the Telos account name is linked to the avatar and link it if it is not (PayWithSeeds will fail if it is not linked when it tries to add the karma points).
        //        if (!STAR.LoggedInAvatar.ProviderUniqueStorageKey.ContainsKey(ProviderType.TelosOASIS))
        //        {
        //            CLIEngine.ShowWorkingMessage("Linking Telos Account to Avatar...");
        //            OASISResult<Guid> linkKeyResult = STAR.OASISAPI.Keys.LinkProviderPublicKeyToAvatarById(Guid.Empty, STAR.LoggedInAvatar.Id, ProviderType.TelosOASIS, "davidsellams");

        //            if (!linkKeyResult.IsError && linkKeyResult.Result != Guid.Empty)
        //                CLIEngine.ShowSuccessMessage($"Telos Account Successfully Linked to Avatar. WalletID: {linkKeyResult.Result}");
        //            else
        //                CLIEngine.ShowErrorMessage($"Error occured Whilst Linking Telos Account To Avatar. Reason: {linkKeyResult.Message}");
        //        }

        //        CLIEngine.ShowWorkingMessage("Sending SEEDS from nextgenworld to davidsellams...");
        //        OASISResult<string> payWithSeedsResult = STAR.OASISAPI.Providers.SEEDS.PayWithSeedsUsingTelosAccount("davidsellams", _privateKey, "nextgenworld", 1, KarmaSourceType.API, "test", "test", "test", "test memo");

        //        if (payWithSeedsResult.IsError)
        //            CLIEngine.ShowErrorMessage(string.Concat("Error Occured: ", payWithSeedsResult.Message));
        //        else
        //            CLIEngine.ShowSuccessMessage(string.Concat("SEEDS Sent. Transaction ID: ", payWithSeedsResult.Result));


        //        CLIEngine.ShowWorkingMessage("Getting Balance for account davidsellams...");
        //        balance = STAR.OASISAPI.Providers.SEEDS.GetBalanceForTelosAccount("davidsellams");
        //        CLIEngine.ShowSuccessMessage(string.Concat("Balance: ", balance));

        //        CLIEngine.ShowWorkingMessage("Getting Balance for account nextgenworld...");
        //        balance = STAR.OASISAPI.Providers.SEEDS.GetBalanceForTelosAccount("nextgenworld");
        //        CLIEngine.ShowSuccessMessage(string.Concat("Balance: ", balance));

        //        CLIEngine.ShowWorkingMessage("Getting Organsiations...");
        //        string orgs = STAR.OASISAPI.Providers.SEEDS.GetAllOrganisationsAsJSON();
        //        CLIEngine.ShowSuccessMessage(string.Concat("Organisations: ", orgs));

        //        //CLIEngine.ShowErrorMessage("Getting nextgenworld organsiation...");
        //        //string org = OASISAPI.Providers.SEEDS.GetOrganisation("nextgenworld");
        //        //CLIEngine.ShowErrorMessage(string.Concat("nextgenworld org: ", org));

        //        CLIEngine.ShowWorkingMessage("Generating QR Code for davidsellams...");
        //        string qrCode = STAR.OASISAPI.Providers.SEEDS.GenerateSignInQRCode("davidsellams");
        //        CLIEngine.ShowSuccessMessage(string.Concat("SEEDS Sign-In QRCode: ", qrCode));

        //        CLIEngine.ShowWorkingMessage("Sending invite to davidsellams...");
        //        OASISResult<SendInviteResult> sendInviteResult = STAR.OASISAPI.Providers.SEEDS.SendInviteToJoinSeedsUsingTelosAccount("davidsellams", _privateKey, "davidsellams", 1, 1, KarmaSourceType.API, "test", "test", "test");
        //        CLIEngine.ShowSuccessMessage(string.Concat("Success: ", sendInviteResult.IsError ? "false" : "true"));

        //        if (sendInviteResult.IsError)
        //            CLIEngine.ShowErrorMessage(string.Concat("Error Message: ", sendInviteResult.Message));
        //        else
        //        {
        //            CLIEngine.ShowSuccessMessage(string.Concat("Invite Sent To Join SEEDS. Invite Secret: ", sendInviteResult.Result.InviteSecret, ". Transction ID: ", sendInviteResult.Result.TransactionId));

        //            CLIEngine.ShowWorkingMessage("Accepting invite to davidsellams...");
        //            OASISResult<string> acceptInviteResult = STAR.OASISAPI.Providers.SEEDS.AcceptInviteToJoinSeedsUsingTelosAccount("davidsellams", sendInviteResult.Result.InviteSecret, KarmaSourceType.API, "test", "test", "test");
        //            CLIEngine.ShowSuccessMessage(string.Concat("Success: ", acceptInviteResult.IsError ? "false" : "true"));

        //            if (acceptInviteResult.IsError)
        //                CLIEngine.ShowErrorMessage(string.Concat("Error Message: ", acceptInviteResult.Message));
        //            else
        //                CLIEngine.ShowSuccessMessage(string.Concat("Invite Accepted To Join SEEDS. Transction ID: ", acceptInviteResult.Result));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CLIEngine.ShowErrorMessage($"Error occured during SEEDS Tests: {ex.Message}");
        //    }

        //    CLIEngine.ShowSuccessMessage("SEEDS Tests Completed.");


        //    // ThreeFold, AcivityPub, SOLID, Cross/Off Chain, Smart Contract Interoperability & lots more coming soon! :)

        //    CLIEngine.ShowSuccessMessage("OASIS API TESTS COMPLETE.");
        //    // END OASIS API DEMO ***********************************************************************************
        //}


        //private static void ShowAvatars(IEnumerable<IAvatar> avatars)
        //{
        //    foreach (IAvatar avatar in avatars)
        //        ShowAvatar(avatar);
        //}

        //private static void ShowAvatar(IAvatar avatar, IAvatarDetail avatarDetail)
        //{
        //    if (avatar != null)
        //    {
        //        //CLIEngine.ShowSuccessMessage("Avatar Loaded Successfully");
        //        CLIEngine.ShowMessage($"Avatar ID: {avatar.Id}");
        //        CLIEngine.ShowMessage($"Avatar Name: {avatar.FullName}");
        //        CLIEngine.ShowMessage($"Avatar Username: {avatar.Username}");
        //        CLIEngine.ShowMessage($"Avatar Type: {avatar.AvatarType.Name}");
        //        CLIEngine.ShowMessage($"Avatar Created Date: {avatar.CreatedDate}");
        //        CLIEngine.ShowMessage($"Avatar Modifed Date: {avatar.ModifiedDate}");
        //        CLIEngine.ShowMessage($"Avatar Last Beamed In Date: {avatar.LastBeamedIn}");
        //        CLIEngine.ShowMessage($"Avatar Last Beamed Out Date: {avatar.LastBeamedOut}");
        //        CLIEngine.ShowMessage(String.Concat("Avatar Is Active: ", avatar.IsActive ? "True" : "False"));
        //        CLIEngine.ShowMessage(String.Concat("Avatar Is Beamed In: ", avatar.IsBeamedIn ? "True" : "False"));
        //        CLIEngine.ShowMessage(String.Concat("Avatar Is Verified: ", avatar.IsVerified ? "True" : "False"));
        //        CLIEngine.ShowMessage($"Avatar Version: {avatar.Version}");

        //        if (CLIEngine.GetConfirmation($"Do you wish to view more detailed information?"))
        //            ShowAvatarStats(avatar, avatarDetail);
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage("Error Loading Avatar.");
        //}

        //TODO: Not sure what this is used for?! :)
        private static void EnableOrDisableAutoProviderList(Func<bool, List<ProviderType>, bool> funct, bool isEnabled, List<ProviderType> providerTypes, string workingMessage, string successMessage, string errorMessage)
        {
            CLIEngine.ShowWorkingMessage(workingMessage);

            if (funct(isEnabled, providerTypes))
                CLIEngine.ShowSuccessMessage(successMessage);
            else
                CLIEngine.ShowErrorMessage(errorMessage);
        }

        //private static void HandleBooleansResponse(bool isSuccess, string successMessage, string errorMessage)
        //{
        //    if (isSuccess)
        //        CLIEngine.ShowSuccessMessage(successMessage);
        //    else
        //        CLIEngine.ShowErrorMessage(errorMessage);
        //}

        //private static void HandleOASISResponse<T>(OASISResult<T> result, string successMessage, string errorMessage)
        //{
        //    if (!result.IsError && result.Result != null)
        //        CLIEngine.ShowSuccessMessage(successMessage);
        //    else
        //        CLIEngine.ShowErrorMessage($"{errorMessage}Reason: {result.Message}");
        //}

        //private static void HandleHolonsOASISResponse(OASISResult<IEnumerable<IHolon>> result)
        //{
        //    if (!result.IsError && result.Result != null)
        //    {
        //        CLIEngine.ShowSuccessMessage($"{result.Result.Count()} Holon(s) Loaded:");
        //        ShowHolons(result.Result, false);
        //    }
        //    else
        //        CLIEngine.ShowErrorMessage($"Error Loading Holons. Reason: {result.Message}");
        //}

        //private static async Task<OASISResult<CoronalEjection>> GenerateCelestialBody(string name, ICelestialBody parentCelestialBody, OAPPType OAPPType, GenesisType genesisType, string celestialBodyDNAFolder = "", string genesisFolder = "", string genesisNameSpace = "")
        //{
        //    // Create (OApp) by generating dynamic template/scaffolding code.
        //    string message = $"Generating {Enum.GetName(typeof(GenesisType), genesisType)} '{name}' (OApp)";

        //    if (genesisType == GenesisType.Moon && parentCelestialBody != null)
        //        message = $"{message} For Planet '{parentCelestialBody.Name}'";

        //    message = $"{message} ...";

        //    CLIEngine.ShowWorkingMessage(message);

        //    //Allows the celestialBodyDNAFolder, genesisFolder & genesisNameSpace params to be passed in overridng what is in the STARDNA.json file.
        //    OASISResult<CoronalEjection> lightResult = STAR.LightAsync(name, OAPPType, genesisType, celestialBodyDNAFolder, genesisFolder, genesisNameSpace, parentCelestialBody).Result;

        //    //Will use settings in the STARDNA.json file.
        //    //OASISResult<CoronalEjection> lightResult = STAR.LightAsync(OAPPType, genesisType, name, parentCelestialBody).Result;

        //    if (lightResult.IsError)
        //        CLIEngine.ShowErrorMessage(string.Concat(" ERROR OCCURED. Error Message: ", lightResult.Message));
        //    else
        //    {
        //        CLIEngine.ShowSuccessMessage($"{Enum.GetName(typeof(GenesisType), genesisType)} Generated.");

        //        Console.WriteLine("");
        //        Console.WriteLine(string.Concat(" Id: ", lightResult.Result.CelestialBody.Id));
        //        Console.WriteLine(string.Concat(" CreatedByAvatarId: ", lightResult.Result.CelestialBody.CreatedByAvatarId));
        //        Console.WriteLine(string.Concat(" CreatedDate: ", lightResult.Result.CelestialBody.CreatedDate));
        //        Console.WriteLine("");
        //        ShowZomesAndHolons(lightResult.Result.CelestialBody.CelestialBodyCore.Zomes, string.Concat($" {Enum.GetName(typeof(GenesisType), genesisType)} contains ", lightResult.Result.CelestialBody.CelestialBodyCore.Zomes.Count(), " Zome(s): "));
        //    }

        //    return lightResult;
        //}

        //private static async Task<OASISResult<CoronalEjection>> GenerateZomesAndHolons(string oAPPName, OAPPType OAPPType, string zomesAndHolonsyDNAFolder = "", string genesisFolder = "", string genesisNameSpace = "")
        //{
        //    // Create (OApp) by generating dynamic template/scaffolding code.
        //    CLIEngine.ShowWorkingMessage($"Generating Zomes & Holons...");

        //    //OASISResult<CoronalEjection> lightResult = STAR.LightAsync(oAPPName, OAPPType, zomesAndHolonsyDNAFolder, genesisFolder, genesisNameSpace).Result;
        //    OASISResult<CoronalEjection> lightResult = STAR.LightAsync(oAPPName, OAPPType, zomesAndHolonsyDNAFolder, genesisFolder, genesisNameSpace).Result;

        //    //Will use settings in the STARDNA.json file.
        //    //OASISResult<CoronalEjection> lightResult = STAR.LightAsync(oAPPName, OAPPType).Result;

        //    if (lightResult.IsError)
        //        CLIEngine.ShowErrorMessage(string.Concat(" ERROR OCCURED. Error Message: ", lightResult.Message));
        //    else
        //    {
        //        int iNoHolons = 0;
        //        foreach (IZome zome in lightResult.Result.Zomes)
        //            iNoHolons += zome.Children.Count();

        //        CLIEngine.ShowSuccessMessage($"{lightResult.Result.Zomes.Count} Zomes & {iNoHolons} Holons Generated.");

        //        Console.WriteLine("");
        //        ShowZomesAndHolons(lightResult.Result.Zomes);
        //    }

        //    return lightResult;
        //}

        //private static void ShowZomesAndHolons(IEnumerable<IZome> zomes, string customHeader = null, string indentBuffer = " ")
        //{
        //    if (string.IsNullOrEmpty(customHeader))
        //        Console.WriteLine($" {zomes.Count()} Zome(s) Found", zomes.Count() > 0 ? ":" : "");
        //    else
        //        Console.WriteLine(customHeader);

        //    Console.WriteLine("");

        //    foreach (IZome zome in zomes)
        //    {
        //        //Console.WriteLine(string.Concat("  | ZOME | Name: ", zome.Name.PadRight(20), " | Id: ", zome.Id, " | Containing ", zome.Children.Count(), " Holon(s)", zome.Children.Count > 0 ? ":" : ""));
        //        string tree = string.Concat(" |", indentBuffer, "ZOME").PadRight(22);
        //        string children = string.Concat(" | Containing ", zome.Children != null ? zome.Children.Count() : 0, " Child Holon(s)");

        //        Console.WriteLine(string.Concat(tree, " | Name: ", zome.Name.PadRight(40), " | Id: ", zome.Id, " | Type: ", "Zome".PadRight(15), children.PadRight(30), " |".PadRight(30), "|"));
        //        ShowHolons(zome.Children, false);
        //    }
        //}

        //private static void ShowHolons(IEnumerable<IHolon> holons, bool showHeader = true, string customHeader = null, int indentBy = 2, int level = 0)
        //{
        //    //Console.WriteLine("");

        //    if (showHeader)
        //    {
        //        if (string.IsNullOrEmpty(customHeader))
        //            Console.WriteLine(string.Concat(" ", holons.Count(), " Child Holons(s) Found", holons.Count() > 0 ? ":" : ""));
        //        else
        //            Console.WriteLine(customHeader);
        //    }

        //    //Console.WriteLine("");
        //    string indentPadding = "";

        //    for (int i = 0; i <= indentBy; i++)
        //        indentPadding = indentPadding.Insert(0, " ");

        //    //  int parentIndent = indentBy;
        //    foreach (IHolon holon in holons)
        //    {
        //        // indentBy = parentIndent;
        //        Console.WriteLine("");
        //        ShowHolonBasicProperties(holon, "", indentPadding, true);
        //        //Console.WriteLine(string.Concat("   Holon Name: ", holon.Name, " Holon Id: ", holon.Id, ", Holon Type: ", Enum.GetName(typeof(HolonType), holon.HolonType), " containing ", holon.Nodes != null ? holon.Nodes.Count() : 0, " node(s): "));

        //        if (holon.Nodes != null)
        //        {
        //            foreach (API.Core.Interfaces.INode node in holon.Nodes)
        //            {
        //                Console.WriteLine("");
        //                string tree = string.Concat(" |", indentPadding, "  NODE").PadRight(22);
        //                //Console.WriteLine(string.Concat(indentPadding, "  | NODE | Name: ", node.NodeName.PadRight(20), " | Id: ", node.Id, " | Type: ", Enum.GetName(node.NodeType).PadRight(10)));
        //                Console.WriteLine(string.Concat(tree, " | Name: ", node.NodeName.PadRight(40), " | Id: ", node.Id, " | Type: ", Enum.GetName(node.NodeType).PadRight(15), " | ".PadRight(30), " | ".PadRight(30), "|"));
        //            }
        //        }

        //        if (holon.Children != null && holon.Children.Count > 0)
        //        {
        //            //indentBy += 2;
        //            //ShowHolons(holon.Children, showHeader, $"{indentPadding}{holon.Children.Count} Child Sub-Holon(s) Found:", indentBy);
        //            ShowHolons(holon.Children, false, "", indentBy + 2, level + 1);
        //        }
        //    }

        //    if (level == 0)
        //        Console.WriteLine("");
        //}

        //private static void ShowHolonBasicProperties(IHolon holon, string prefix = "", string indentBuffer = " ", bool showChildren = true, bool showNodes = true)
        //{
        //    string children = "";
        //    string nodes = "";

        //    if (showChildren)
        //        children = string.Concat(" | Containing ", holon.Children != null ? holon.Children.Count() : 0, " Child Holon(s)");
        //    else
        //        children = " |";

        //    if (showNodes)
        //        nodes = string.Concat(" | Containing ", holon.Nodes != null ? holon.Nodes.Count() : 0, " Node(s)");
        //    else
        //        nodes = " |";

        //    string tree = string.Concat(" |", indentBuffer, "HOLON").PadRight(22);

        //    //Console.WriteLine(string.Concat(indentBuffer, prefix, "| HOLON | Name: ", holon.Name.PadRight(20), prefix, " | Id: ", holon.Id, prefix, " | Type: ", Enum.GetName(typeof(HolonType), holon.HolonType).PadRight(10), children, nodes));
        //    Console.WriteLine(string.Concat(tree, " | Name: ", holon.Name != null ? holon.Name.PadRight(40) : "".PadRight(40), prefix, " | Id: ", holon.Id, prefix, " | Type: ", Enum.GetName(typeof(HolonType), holon.HolonType).PadRight(15), children.PadRight(30), nodes.PadRight(30), "|"));
        //}

      

        //private static string GetValidEmail(string message, bool checkIfEmailAlreadyInUse)
        //{
        //    bool emailValid = false;
        //    string email = "";

        //    while (!emailValid)
        //    {
        //        CLIEngine.ShowMessage(string.Concat("", message), true, true);
        //        email = Console.ReadLine();

        //        if (!ValidationHelper.IsValidEmail(email))
        //            CLIEngine.ShowErrorMessage("That email is not valid. Please try again.");

        //        else if (checkIfEmailAlreadyInUse)
        //        {
        //            CLIEngine.ShowWorkingMessage("Checking if email already in use...");
        //            CLIEngine.SupressConsoleLogging = true;

        //            OASISResult<bool> checkIfEmailAlreadyInUseResult = STAR.OASISAPI.Avatar.CheckIfEmailIsAlreadyInUse(email);
        //            CLIEngine.SupressConsoleLogging = false;

        //            //if (!checkIfEmailAlreadyInUseResult.Result)
        //            //{
        //            //    emailValid = true;
        //            //    CLIEngine.Spinner.Stop();
        //            //    CLIEngine.ShowMessage("", false);
        //            //}

        //            //No need to show error message because the CheckIfEmailIsAlreadyInUse function already shows this! ;-)
        //            if (checkIfEmailAlreadyInUseResult.Result)
        //                CLIEngine.ShowErrorMessage(checkIfEmailAlreadyInUseResult.Message);
        //            else
        //            {
        //                emailValid = true;
        //                CLIEngine.Spinner.Stop();
        //                CLIEngine.ShowMessage("", false);
        //            }
        //        }
        //        else
        //            emailValid = true;
        //    }

        //    return email;
        //}

        //private static string GetValidUsername(string message, bool checkIfUsernameAlreadyInUse = true)
        //{
        //    bool usernameValid = false;
        //    string username = "";

        //    while (!usernameValid)
        //    {
        //        CLIEngine.ShowMessage(string.Concat("", message), true, true);
        //        username = Console.ReadLine();

        //        if (checkIfUsernameAlreadyInUse)
        //        {
        //            CLIEngine.ShowWorkingMessage("Checking if username already in use...");
        //            CLIEngine.SupressConsoleLogging = true;

        //            OASISResult<bool> checkIfUsernameAlreadyInUseResult = STAR.OASISAPI.Avatar.CheckIfUsernameIsAlreadyInUse(username);
        //            CLIEngine.SupressConsoleLogging = false;

        //            //if (!checkIfUsernameAlreadyInUseResult.Result)
        //            //{
        //            //    usernameValid = true;
        //            //    CLIEngine.Spinner.Stop();
        //            //    CLIEngine.ShowMessage("", false);
        //            //}

        //            //No need to show error message because the CheckIfUsernameIsAlreadyInUse function already shows this! ;-)
        //            if (checkIfUsernameAlreadyInUseResult.Result)
        //                CLIEngine.ShowErrorMessage(checkIfUsernameAlreadyInUseResult.Message);
        //            else
        //            {
        //                usernameValid = true;
        //                CLIEngine.Spinner.Stop();
        //                CLIEngine.ShowMessage("", false);
        //            }
        //        }
        //        else
        //            usernameValid = true;
        //    }

        //    return username;
        //}

        //private static bool CreateAvatar()
        //{
        //    ConsoleColor favColour = ConsoleColor.Green;
        //    ConsoleColor cliColour = ConsoleColor.Green;

        //    CLIEngine.ShowMessage("");
        //    CLIEngine.ShowMessage("Please create an avatar below:", false);

        //    string title = CLIEngine.GetValidTitle("What is your title? ");
        //    string firstName = CLIEngine.GetValidInput("What is your first name? ");
        //    CLIEngine.ShowMessage(string.Concat("Nice to meet you ", firstName, ". :)"));
        //    string lastName = CLIEngine.GetValidInput(string.Concat("What is your last name ", firstName, "? "));
        //    string email = GetValidEmail("What is your email address? ", true);
        //    string username = GetValidUsername("What username would you like? ", true);
        //    CLIEngine.GetValidColour(ref favColour, ref cliColour);
        //    string password = CLIEngine.GetValidPassword();
        //    CLIEngine.ShowWorkingMessage("Creating Avatar...");

        //    CLIEngine.SupressConsoleLogging = true;
        //    OASISResult<IAvatar> createAvatarResult = Task.Run(async () => await STAR.CreateAvatarAsync(title, firstName, lastName, email, username, password, cliColour, favColour)).Result;
        //    //OASISResult<IAvatar> createAvatarResult = STAR.CreateAvatar(title, firstName, lastName, email, username, password, cliColour, favColour);
        //    CLIEngine.SupressConsoleLogging = false;
        //    CLIEngine.ShowMessage("");

        //    if (createAvatarResult.IsError)
        //    {
        //        CLIEngine.ShowErrorMessage(string.Concat("Error creating avatar. Error message: ", createAvatarResult.Message));
        //        return false;
        //    }
        //    else
        //    {
        //        CLIEngine.ShowSuccessMessage("Successfully Created Avatar. Please Check Your Email To Verify Your Account Before Logging In.");
        //        return true;
        //    }
        //}

        private static void ShowHeader()
        {
            //Assembly assembly = typeof(Program).Assembly;
            //System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            //string versionString = fvi.FileVersion;

            // Console.SetWindowSize(300, Console.WindowHeight);
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("*************************************************************************************************");
            Console.Write(" NextGen Software");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" STAR");
            Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write($" (Synergiser Transformer Aggregator Resolver) HDK/ODK TEST HARNESS v{versionString} ");

            if (RandomNumberGenerator.GetInt32(1) == 0)
                Console.Write($" (Synergiser Transformer Aggregator Resolver) HDK/ODK {OASISBootLoader.OASISBootLoader.STARODKVersion} ");
            else
                Console.Write($" (Super Technogically Advanced Reality-Engine) HDK/ODK {OASISBootLoader.OASISBootLoader.STARODKVersion} ");

            Console.WriteLine("");
            Console.WriteLine("*************************************************************************************************");
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("                  ,O,");
            Console.WriteLine("                 ,OOO,");
            Console.WriteLine("           'oooooOOOOOooooo'");
            Console.WriteLine("             `OOOOOOOOOOO`");
            Console.WriteLine("               `OOOOOOO`");
            Console.WriteLine("               OOOO'OOOO");
            Console.WriteLine("              OOO'   'OOO");
            Console.WriteLine("             O'         'O");

            /*
            Image Picture = Image.FromFile("images/star6b.jpg");
            Console.SetBufferSize((Picture.Width * 0x2), (Picture.Height * 0x2));
            //Console.SetBufferSize((Picture.Width), (Picture.Height));
            Console.WindowWidth = 100; //180
            //Console.WindowHeight = 61;

            FrameDimension Dimension = new FrameDimension(Picture.FrameDimensionsList[0x0]);
            int FrameCount = Picture.GetFrameCount(Dimension);
            int Left = Console.WindowLeft, Top = Console.WindowTop;
            char[] Chars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' ' };
            Picture.SelectActiveFrame(Dimension, 0x0);
            for (int i = 0x0; i < Picture.Height; i++)
            {
                for (int x = 0x0; x < Picture.Width; x++)
                {
                    Color Color = ((Bitmap)Picture).GetPixel(x, i);
                    int Gray = (Color.R + Color.G + Color.B) / 0x3;
                    int Index = (Gray * (Chars.Length - 0x1)) / 0xFF;
                    Console.Write(Chars[Index]);
                }
                Console.Write('\n');
                Thread.Sleep(50);
            }
            //Console.SetCursorPosition(Left, Top);
            */

            // Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
            Colorful.Console.WriteAscii(" STAR", Color.Yellow);

            // var font = FigletFont.Load("fonts/wow.flf");
            // Figlet figlet = new Figlet(font);
            //Colorful.Console.WriteLine(figlet.ToAscii("STAR"), Color.FromArgb(67, 144, 198));
            // Colorful.Console.WriteLine(figlet.ToAscii("STAR"), Color.Yellow);

            ShowCommands();

            Console.WriteLine("");
            Console.Write(" Welcome to");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" STAR");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" (The");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" ♥");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" Of The OASIS)");
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        private static void ShowCommands(bool showFullCommands = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n USAGE:");
            Console.WriteLine("    star {SUBCOMMAND}");
            Console.WriteLine("");
            Console.WriteLine(" FLAGS:");
            Console.WriteLine("    ignite           Ignite STAR & Boot The OASIS");
            Console.WriteLine("    extinguish       Extinguish STAR & Shutdown The OASIS");
            //Console.WriteLine("    beamin         Log in");
            //Console.WriteLine("    beamout        Log out");
            Console.WriteLine("    help [full]      Show this help page. If the [full] flag is omitted it will show only the top level sub-commands, if [full] is included it will show every option for each sub-command.");
            Console.WriteLine("    version          Show the versions of STAR ODK, COSMIC ORM, OASIS Runtime & the OASIS Providers..");
            Console.WriteLine("    status           Show the status of STAR ODK.");
            Console.WriteLine("    exit             Exit the STAR CLI.");
            Console.WriteLine("");
            Console.WriteLine(" SUBCOMMANDS:");

            if (showFullCommands)
            {
                Console.WriteLine("    light                            {OAPPName} {OAPPDesc} {OAPPType}       Creates a new OAPP (Zomes/Holons/Star/Planet/Moon) at the given genesis folder location, from the given OAPP DNA.");
                Console.WriteLine("                                     {dnaFolder} {geneisFolder}");
                Console.WriteLine("                                     {genesisNameSpace} {genesisType}");
                Console.WriteLine("                                     {parentCelestialBodyId}");
                Console.WriteLine("    light                                                                   Displays more detail on how to use this command and optionally launches the Light Wizard.");
                Console.WriteLine("    light wiz                                                               Start the Light Wizard.");
                Console.WriteLine("    light transmute                  {hAppDNA} {geneisFolder}               Creates a new Planet (OApp) at the given folder genesis locations, from the given hApp DNA.");
                Console.WriteLine("    bang                                                                    Generate a whole metaverse or part of one such as Multierveres, Universes, Dimensions, Galaxy Clusters, Galaxies, Solar Systems, Stars, Planets, Moons etc.");
                Console.WriteLine("    wiz                                                                     Start the STAR ODK Wizard which will walk you through the steps for creating a OAPP tailored to your specefic needs");
                Console.WriteLine("                                                                            (such as which OASIS Providers do you need and the specefic use case(s) you need etc).");
                Console.WriteLine("    flare                            {id/name}                              Build a OAPP for the given {id} or {name}.");
                Console.WriteLine("    shine                            {id/name}                              Launch & activate a OAPP for the given {id} or {name} by shining the 's light upon it..."); //TODO: Dev next.
                Console.WriteLine("    twinkle                          {id/name}                              Activate a published OAPP for the given {id} or {name} within the STARNET store."); //TODO: Dev next.
                Console.WriteLine("    dim                              {id/name}                              Deactivate a published OAPP for the given {id} or {name} within the STARNET store."); //TODO: Dev next.
                Console.WriteLine("    seed                             {id/name}                              Deploy/Publish a OAPP for the given {id} or {name} to the STARNET Store.");
                Console.WriteLine("    unseed                           {id/name}                              Undeploy/Unpublish a OAPP for the given {id} or {name} from the STARNET Store.");
                Console.WriteLine("    dust                             {id/name}                              Delete a OAPP for the given {id} or {name} (this will also remove it from STARNET if it has already been published)."); //TODO: Dev next.
                Console.WriteLine("    radiate                          {id/name}                              Highlight the OAPP for the given {id} or {name} in the STARNET Store. *Admin/Wizards Only*");
                Console.WriteLine("    emit                             {id/name}                              Show how much light the OAPP is emitting into the solar system for the given {id} or {name}");
                Console.WriteLine("                                                                            (this is determined by the collective karma score of all users of that OAPP).");
                Console.WriteLine("    reflect                          {id/name}                              Show stats of the OAPP for the given {id} or {name}.");
                Console.WriteLine("    evolve                           {id/name}                              Upgrade/update a OAPP) for the given {id} or {name}."); //TODO: Dev next.
                Console.WriteLine("    mutate                           {id/name}                              Import/Export hApp, dApp & others for the given {id} or {name}.");
                Console.WriteLine("    love                             {id/username}                          Send/Receive Love for the given {id} or {username}.");
                Console.WriteLine("    burst                                                                   View network stats/management/settings.");
                Console.WriteLine("    super                                                                   Reserved For Future Use...");
                //Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's, zomes, holons, celestial spaces, celestial bodies, geo-nft's, geo-hotspots, missions, chapters, quests & inventory items.");
                Console.WriteLine("    net                                                                     Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's & more!");

                Console.WriteLine("    avatar beamin                                                           Beam in (log in).");
                Console.WriteLine("    avatar beamout                                                          Beam out (Log out).");
                Console.WriteLine("    avatar whoisbeamedin                                                    Display who is currently beamed in (if any) and the last time they beamed in and out.");
                Console.WriteLine("    avatar show me                                                          Display the currently beamed in avatar details (if any).");
                Console.WriteLine("    avatar show                      {id/username}                          Shows the details for the avatar for the given {id} or {username}.");
                Console.WriteLine("    avatar edit                                                             Edit the currently beamed in avatar.");
                Console.WriteLine("    avatar list                                                             Loads all avatars.");
                Console.WriteLine("    avatar search                                                           Seach avatars that match the given seach parameters (public fields only such as level, karma, username & any fields the player has set to public).");
                Console.WriteLine("    karma list                                                              Display the karma thresholds.");
                Console.WriteLine("    keys link                                                               Links a OASIS Provider Key to the current beamed in avatar.");
                Console.WriteLine("    keys list                                                               Shows the keys for the current beamed in avatar.");
                Console.WriteLine("    wallet list                                                             Shows the wallets for the current beamed in avatar.");
                Console.WriteLine("    search                                                                  Seaches The OASIS for the given seach parameters.");
                Console.WriteLine("    oapp create                                                             Shortcut to the light sub-command.");
                Console.WriteLine("    oapp update                      {id/name}                              Update an existing OAPP for the given {id} or {name}.");
                Console.WriteLine("    oapp delete                      {id/name}                              Delete an existing OAPP for the given {id} or {name}.");
                Console.WriteLine("    oapp install                     {id/name}                              Install a OAPP for the given {id} or {name}.");
                Console.WriteLine("    oapp uninstall                   {id/name}                              Uninstall a OAPP for the given {id} or {name}.");
                Console.WriteLine("    oapp publish                     {id/name}                              Shortcut to the seed sub-command.");
                Console.WriteLine("    oapp unpublish                   {id/name}                              Shortcut to the un-seed sub-command.");
                Console.WriteLine("    oapp show                        {id/name}                              Shows a OAPP for the given {id} or {name}.");
                Console.WriteLine("    oapp list                        [all]                                  List all OAPPs (contains zomes and holons) that have been generated.");
                Console.WriteLine("    oapp list installed                                                     List all OAPP's installed for the current beamed in avatar.");
                Console.WriteLine("    oapp search                      [all]                                  Searches the OAPP's for the given search critera.");
                Console.WriteLine("    oapp template create                                                    Creates a OAPP template.");
                Console.WriteLine("    oapp template update             {id/name}                              Updates a OAPP template for the given {id} or {name}.");
                Console.WriteLine("    oapp template delete             {id/name}                              Deletes a OAPP template for the given {id} or {name}.");
                Console.WriteLine("    oapp template install            {id/name}                              Installs a OAPP template for the given {id} or {name}.");
                Console.WriteLine("    oapp template uninstall          {id/name}                              Uninstalls a OAPP template for the given {id} or {name}.");
                Console.WriteLine("    oapp template publish            {id/name}                              Publishes a OAPP template to the STARNET store for the given {id} or {name}.");
                Console.WriteLine("    oapp template unpublish          {id/name}                              Unpublishes a OAPP template from the STARNET store for the given {id} or {name}.");
                Console.WriteLine("    oapp template show               {id/name}                              Shows a OAPP template for the given {id} or {name}.");
                Console.WriteLine("    oapp template list               [all]                                  List all OAPP templates that have been created.");
                Console.WriteLine("    oapp template list installed                                            List all OAPP templates installed for the current beamed in avatar.");
                Console.WriteLine("    oapp template search             [all]                                  Searches the OAPP templates for the given search critera.");
                Console.WriteLine("    happ create                                                             Shortcut to the light sub-command.");
                Console.WriteLine("    happ update                      {id/name}                              Update an existing hApp for the given {id} or {name}.");
                Console.WriteLine("    happ delete                      {id/name}                              Delete an existing hApp for the given {id} or {name}.");
                Console.WriteLine("    happ publish                     {id/name}                              Shortcut to the seed sub-command.");
                Console.WriteLine("    happ unpublish                   {id/name}                              Shortcut to the un-seed sub-command.");
                Console.WriteLine("    happ show                        {id/name}                              Shows a hApp for the given {id} or {name}.");
                Console.WriteLine("    happ list                        [all]                                  List all hApps (contains zomes) that have been generated.");
                Console.WriteLine("    happ list installed                                                     List all hApp's installed for the current beamed in avatar.");
                Console.WriteLine("    happ search                      [all]                                  Searches the hApp's for the given search critera.");
                Console.WriteLine("    celestialspace create                                                   Creates a celestial space.");
                Console.WriteLine("    celestialspace update            {id/name}                              Update an existing celestial space for the given {id} or {name}.");
                Console.WriteLine("    celestialspace delete            {id/name}                              Delete an existing celestial space for the given {id} or {name}.");
                Console.WriteLine("    celestialspace publish           {id/name}                              Publishes a celestial space for the given {id} or {name} to the STARNET store so others can use in their own OAPP's etc.");
                Console.WriteLine("    celestialspace unpublish         {id/name}                              Unpublishes a celestial space for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    celestialspace show              {id/name}                              Shows a celestial space for the given {id} or {name}.");
                Console.WriteLine("    celestialspace list              [all]                                  List all celestial spaces that have been generated.");
                Console.WriteLine("    celestialspace search            [all]                                  Searches the celestial spaces for the given search critera.");
                Console.WriteLine("    celestialbody create                                                    Creates a celestial body.");
                Console.WriteLine("    celestialbody update             {id/name}                              Update an existing celestial body for the given {id} or {name}.");
                Console.WriteLine("    celestialbody delete             {id/name}                              Delete an existing celestial body for the given {id} or {name}.");
                Console.WriteLine("    celestialbody publish            {id/name}                              Publishes a celestial body for the given {id} or {name} to the STARNET store so others can use in their own OAPP's etc.");
                Console.WriteLine("    celestialbody unpublish          {id/name}                              Unpublishes a celestial body for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    celestialbody show               {id/name}                              Shows a celestial body for the given {id} or {name}.");
                Console.WriteLine("    celestialbody list               [all]                                  List all celestial bodies that have been generated.");
                Console.WriteLine("    celestialbody search             [all]                                  Searches the celestial bodies for the given search critera.");
                Console.WriteLine("    zome create                                                             Create a zome (module).");
                Console.WriteLine("    zome update                      {id/name}                              Update an existing zome for the given {id} or {name}");
                Console.WriteLine("                                                                            (can upload a zome.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                Console.WriteLine("    zome delete                      {id/name}                              Delete an existing zome for the given {id} or {name}.");
                Console.WriteLine("    zome publish                     {id/name}                              Publishes a zome for the given {id} or {name} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                Console.WriteLine("    zome unpublish                   {id/name}                              Unpublishes a zome for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    zome show                        {id/name}                              Shows a zome for the given {id} or {name}.");
                Console.WriteLine("    zome list                        [all]                                  List all zomes (modules that contain holons) that have been generated.");
                Console.WriteLine("    zome search                      [all]                                  Searches the zomes (modules) for the given search critera. If [all] is omitted it will search only your zomes otherwise it will search all public/shared zomes.");
                //Console.WriteLine("    holon = Shows more info on how to use this command and optionally lauches the Save Holon Wizard.");
                Console.WriteLine("    holon create                     json={holonJSONFile}                   Creates/Saves a holon from the given {holonJSONFile}.");
                Console.WriteLine("    holon create wiz                                                        Starts the Create Holon Wizard.");
                Console.WriteLine("    holon update                     {id/name}                              Update an existing holon for the given {id} or {name}");
                Console.WriteLine("                                                                            (can upload a holon.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                Console.WriteLine("    holon delete                     {id/name}                              Deletes a holon for the given {id} or {name}.");
                Console.WriteLine("    holon publish                    {id/name}                              Publishes a holon for the given {id} or {name} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                Console.WriteLine("    holon unpublish                  {id/name}                              Unpublishes a holon for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    holon show                       {id/name}                              Shows a holon for the given {id} or {name}.");
                Console.WriteLine("    holon list                       [all]                                  List all holons (OASIS Data Objects) that have been generated.");
                Console.WriteLine("    holon search                     [all]                                  Searches the holons for the given search critera.");
                Console.WriteLine("    chapter create                                                          Creates a chapter that can be linked to a mission. Quests can be added to the chapter. Chapters are used to group quests together (optional).");
                Console.WriteLine("    chapter update                   {id/name}                              Updates a chapter for the given {id} or {name}.");
                Console.WriteLine("    chapter delete                   {id/name}                              Deletes a chapter for the given {id} or {name}.");
                Console.WriteLine("    chapter publish                  {id/name}                              Publishes a chapter to the STARNET store for the given {id} or {name} so others can use in their own missions.");
                Console.WriteLine("    chapter unpublish                {id/name}                              Unpublishes a chapter from the STARNET store for the given {id} or {name}.");
                Console.WriteLine("    chapter show                     {id/name}                              Shows the chapter for the given {id} or {name}.");
                Console.WriteLine("    chapter list                     [all]                                  List chapters that have been created.");
                Console.WriteLine("    chapter search                   [all]                                  Search chapters that have been created.");
                Console.WriteLine("    mission create                                                          Creates a mission that chapters & quests can be added to.");
                Console.WriteLine("    mission update                   {id/name}                              Updates a mission for the given {id} or {name}.");
                Console.WriteLine("    mission delete                   {id/name}                              Deletes an mission for the given {id} or {name}.");
                Console.WriteLine("    mission publish                  {id/name}                              Publishes a mission  for the given {id} or {name} to the STARNET store so others can find and play in Our World/AR World, One World & any other OASIS OAPP.");
                Console.WriteLine("    mission unpublish                {id/name}                              Unpublishes a mission from the STARNET store for the given {id} or {name}.");
                Console.WriteLine("    mission show                     {id/name}                              Shows the mission for the given {id} or {name}.");
                Console.WriteLine("    missions list                    [all]                                  List all mission's that have been created.");
                Console.WriteLine("    missions search                  [all]                                  Search all mission's that have been created.");
                Console.WriteLine("    quest create                                                            Creates a quest that can be linked to a mission. Geo-nfts, geo-hotspots & rewards can be linked to the quest.");
                Console.WriteLine("    quest update                     {id/name}                              Updates a quest for the given {id} or {name}.");
                Console.WriteLine("    quest delete                     {id/name}                              Deletes a quest for the given {id} or {name}.");
                Console.WriteLine("    quest publish                    {id/name}                              Publishes a quest to the STARNET store so others can use in their own quests as sub-quests or in missions/chapters.");
                Console.WriteLine("    quest unpublish                  {id/name}                              Unpublishes a quest from the STARNET store for the given {id} or {name}.");
                Console.WriteLine("    quest show                       {id/name}                              Shows the quest for the given {id} or {name}.");
                Console.WriteLine("    quest list                       [all]                                  List all quests that have been created.");
                Console.WriteLine("    quest search                     [all]                                  Search all quests that have been created.");
                Console.WriteLine("    nft mint                                                                Mints a OASIS NFT for the current beamed in avatar.");
                Console.WriteLine("    nft update                       {id/name}                              Updates a nft for the given {id} or {name}.");
                Console.WriteLine("    nft burn                         {id/name}                              Burn's a nft for the given {id} or {name}.");
                Console.WriteLine("    nft send                         {id/name}                              Send a NFT for the given {id} or {name} to another wallet cross-chain.");
                Console.WriteLine("    nft publish                      {id/name}                              Publishes a OASIS NFT for the given {id} or {name} to the STARNET store so others can use in their own geo-nft's etc.");
                Console.WriteLine("    nft unpublish                    {id/name}                              Unpublishes a OASIS NFT for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    nft show                         {id/name}                              Shows the NFT for the given {id} or {name}.");
                Console.WriteLine("    nft list                         [all]                                  Shows the NFT's that belong to the current beamed in avatar.");
                Console.WriteLine("    nft search                       [all]                                  Search for NFT's that match certain criteria and belong to the current beamed in avatar.");
                Console.WriteLine("    geonft mint                                                             Mints a OASIS Geo-NFT and places in Our World/AR World for the current beamed in avatar.");
                Console.WriteLine("    geonft update                    {id/name}                              Updates a geo-nft for the given {id} or {name}.");
                Console.WriteLine("    geonft burn                      {id/name}                              Burn's a geo-nft for the given {id} or {name}.");
                Console.WriteLine("    geonft place                     {id/name}                              Places an existing OASIS NFT for the given {id} or {name} in Our World/AR World for the current beamed in avatar.");
                Console.WriteLine("    geonft send                      {id/name}                              Send a geo-nft for the given {id} or {name} to another wallet cross-chain.");
                Console.WriteLine("    geonft publish                   {id/name}                              Publishes a geo-nft for the given {id} or {name} to the STARNET store so others can use in their own quests etc.");
                Console.WriteLine("    geonft unpublish                 {id/name}                              Unpublishes a geo-nft for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    geonft show                      {id/name}                              Shows the Geo-NFT for the given {id} or {name}");
                Console.WriteLine("    geonft list                      [all]                                  List all geo-nft's that have been created. If [all] is omitted it will list only your geo-nft's otherwise it will list all published geo-nft's as well as yours.");
                Console.WriteLine("    geonft search                    [all]                                  Search for Geo-NFT's that match certain criteria and belong to the current beamed in avatar.");
                Console.WriteLine("                                                                            If [all] is used then it will also include any shared/public/published geo-nft's");
                Console.WriteLine("    geohotspot create                                                       Creates a geo-hotspot that chapters & quests can be added to.");
                Console.WriteLine("    geohotspot update                {id/name}                              Updates a geo-hotspot for the given {id} or {name}.");
                Console.WriteLine("    geohotspot delete                {id/name}                              Deletes an geo-hotspot for the given {id} or {name}.");
                Console.WriteLine("    geohotspot publish               {id/name}                              Publishes a geo-hotspot for the given {id} or {name} to the STARNET store so others can use in their own quests.");
                Console.WriteLine("    geohotspot unpublish             {id/name}                              Unpublishes a geo-hotspot from the STARNET store.");
                Console.WriteLine("    geohotspot show                  {id/name}                              Shows the geo-hotspot for the given {id} or {name}.");
                Console.WriteLine("    geohotspots list                 [all]                                  List all geo-hotspot's that have been created.");
                Console.WriteLine("    geohotspots search               [all]                                  Search all geo-hotspot's that have been created.");
                Console.WriteLine("    inventoryitem create                                                    Creates an inventory item that can be granted as a reward");
                Console.WriteLine("                                                                            (will be placed in the avatar's inventory) for completing quests, collecting geo-nft's, triggering geo-hotspots etc.");
                Console.WriteLine("    inventoryitem update             {id/name}                              Updates a inventory item for the given {id} or {name}.");
                Console.WriteLine("    inventoryitem delete             {id/name}                              Deletes a inventory item for the given {id} or {name}.");
                Console.WriteLine("    inventoryitem publish            {id/name}                              Publishes an inventory item for the given {id} or {name} to the STARNET store so others can use in their own quests, geo-hotspots, geo-nfts, etc.");
                Console.WriteLine("    inventoryitem unpublish          {id/name}                              Unpublishes an inventory item  for the given {id} or {name} from the STARNET store.");
                Console.WriteLine("    inventoryitem show               {id/name}                              Shows the inventory item for the given {id} or {name}.");
                Console.WriteLine("    inventoryitem list               [all]                                  List all inventory item's that have been created.");
                Console.WriteLine("    inventoryitem search             [all]                                  Search all inventory item's that have been created.");
                //Console.WriteLine("    inventoryitem activate = Activates an inventory item that has been published to the STARNET store so is visible to others.");
                //Console.WriteLine("    inventoryitem deactivate = Deactivates an inventory item that has been published to the STARNET store so is invisible to others.");
                Console.WriteLine("    seeds balance                    {telosAccountName/avatarId}            Get's the balance of your SEEDS account.");
                Console.WriteLine("    seeds organisations                                                     Get's a list of all the SEEDS organisations.");
                Console.WriteLine("    seeds organisation               {organisationName}                     Get's a organisation for the given {organisationName}.");
                Console.WriteLine("    seeds pay                        {telosAccountName/avatarId}            Pay using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds donate                     {telosAccountName/avatarId}            Donate using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds reward                     {telosAccountName/avatarId}            Reward using SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds invite                     {telosAccountName/avatarId}            Send invite to join SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds accept                     {telosAccountName/avatarId}            Accept the invite to join SEEDS using either your {telosAccountName} or {avatarId} and earn karma.");
                Console.WriteLine("    seeds qrcode                     {telosAccountName/avatarId}            Generate a sign-in QR code using either your {telosAccountName} or {avatarId}.");
                Console.WriteLine("    data save                        {key} {value}                          Saves data for the given {key} and {value} to the currently beamed in avatar.");
                Console.WriteLine("    data load                        {key}                                  Loads data for the given {key} for the currently beamed in avatar.");
                Console.WriteLine("    data delete                      {key}                                  Deletes data for the given {key} for the currently beamed in avatar.");
                Console.WriteLine("    data list                                                               Lists all data for the currently beamed in avatar.");
                Console.WriteLine("    map setprovider                  {mapProviderType}                      Sets the current {mapProviderType}.");
                Console.WriteLine("    map draw3dobject                 {3dObjectPath} {x} {y}                 Draws a 3D object on the map at {x/y} co-ordinates for the given file {3dobjectPath}.");
                Console.WriteLine("    map draw2dsprite                 {2dSpritePath} {x} {y}                 Draws a 2d sprite on the map at {x/y} co-ordinates for the given file {2dSpritePath}.");
                Console.WriteLine("    map draw2dspriteonhud            {2dSpritePath}                         Draws a 2d sprite on the HUD for the given file {2dSpritePath}.");
                Console.WriteLine("    map draw2dspriteonhud            {2dSpritePath}                         Draws a 2d sprite on the HUD for the given file {2dSpritePath}.");
                Console.WriteLine("    oland price                                                             Get the current OLAND price.");
                Console.WriteLine("    oland purchase                                                          Purchase OLAND for Our World/OASIS.");
                Console.WriteLine("    oland load                       {id}                                   Load a OLAND for the given {id}.");
                Console.WriteLine("    oland save                                                              Save a OLAND.");
                Console.WriteLine("    oland delete                     {id}                                   Deletes a OLAND for the given {id}.");
                Console.WriteLine("    oland list                       [all]                                  If [all] is omitted it will list all OLAND for the given beamed in avatar, otherwise it will list all OLAND for all avatars.");
                Console.WriteLine("    onode start                                                             Starts a OASIS Node (ONODE) and registers it on the OASIS Network (ONET).");
                Console.WriteLine("    onode stop                                                              Stops a OASIS Node (ONODE).");
                Console.WriteLine("    onode status                                                            Shows stats for this ONODE.");
                Console.WriteLine("    onode config                                                            Opens the ONODE's OASISDNA to allow changes to be made (you will need to stop and start the ONODE for changes to apply).");
                Console.WriteLine("    onode providers                                                         Shows what OASIS Providers are running for this ONODE.");
                Console.WriteLine("    onode startprovider              {ProviderName}                         Starts a given provider.");
                Console.WriteLine("    onode stopprovider               {ProviderName}                         Stops a given provider.");
                Console.WriteLine("    hypernet start                                                          Starts the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    hypernet stop                                                           Stops the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    hypernet status                                                         Shows stats for the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    hypernet config                                                         Opens the HyperNET's DNA to allow changes to be made (you will need to stop and start the HyperNET Service for changes to apply.");
                Console.WriteLine("    onet status                                                             Shows stats for the OASIS Network (ONET).");
                Console.WriteLine("    onet providers                                                          Shows what OASIS Providers are running across the ONET and on what ONODE's.");
                Console.WriteLine("    config cosmicdetailedoutput      {enable/disable/status}                Enables/disables COSMIC Detailed Output.");
                Console.WriteLine("    config starstatusdetailedoutput  {enable/disable/status}                Enables/disables STAR ODK Detailed Output.");
                Console.WriteLine("    runcosmictests                   {OAPPType} {dnaFolder} {geneisFolder}  Run the STAR ODK/COSMIC Tests... If OAPPType, DNAFolder or GenesisFolder are not specified it will use the defaults.");
                Console.WriteLine("    runoasisapitests                                                        Run the OASIS API Tests...");
                Console.WriteLine("");
                //Console.WriteLine(" NOTES: -  is not needed if using the STAR CLI Console directly. Star is only needed if calling from the command line or another external script ( is simply the name of the exe).");
                Console.WriteLine(" NOTES:");
                Console.WriteLine("        When invoking any sub-commands that take a {id} or {title}, if neither is specified then a wizard will launch to help find the correct item.");
                Console.WriteLine("        When invoking any sub-commands that have an optional [all] argument/flag, if it is omitted it will search only your items, otherwise it will search all published items as well as yours.");
                Console.WriteLine("        If you invoke a sub-command without any arguments it will show more detailed help on how to use that sub-command as well as the option to lanuch any wizards to help guide you.");



                //Console.WriteLine("    light {OAPPName} {OAPPDesc} {OAPPType} {dnaFolder} {geneisFolder} {genesisNameSpace} {genesisTyp} {parentCelestialBodyId} (optional) = Creates a new OAPP (Zomes/Holons/Star/Planet/Moon) at the given genesis folder location, from the given OAPP DNA.");
                //Console.WriteLine("    light = Displays more detail on how to use this command and optionally launches the Light Wizard.");
                //Console.WriteLine("    light wiz = Start the Light Wizard.");
                //Console.WriteLine("    light transmute {hAppDNA} {geneisFolder}  = Creates a new Planet (OApp) at the given folder genesis locations, from the given hApp DNA.");

                //Console.WriteLine("    bang = Generate a whole metaverse or part of one such as Multierveres, Universes, Dimensions, Galaxy Clusters, Galaxies, Solar Systems, Stars, Planets, Moons etc.");
                //Console.WriteLine("    wiz = Start the STAR ODK Wizard which will walk you through the steps for creating a OAPP tailored to your specefic needs (such as which OASIS Providers do you need and the specefic use case(s) you need etc).");
                //Console.WriteLine("    flare {id/name} = Build a OAPP for the given {id} or {name}.");
                //Console.WriteLine("    shine {id/name} = Launch & activate a OAPP for the given {id} or {name} by shining the 's light upon it..."); //TODO: Dev next.
                //Console.WriteLine("    twinkle {id/name} = Activate a published OAPP for the given {id} or {name} within the STARNET store."); //TODO: Dev next.
                //Console.WriteLine("    dim {id/name} = Deactivate a published OAPP for the given {id} or {name} within the STARNET store."); //TODO: Dev next.
                //Console.WriteLine("    seed {id/name} = Deploy/Publish a OAPP for the given {id} or {name} to the STARNET Store.");
                //Console.WriteLine("    unseed {id/name} = Undeploy/Unpublish a OAPP for the given {id} or {name} from the STARNET Store.");
                //Console.WriteLine("    dust {id/name} = Delete a OAPP for the given {id} or {name} (this will also remove it from STARNET if it has already been published)."); //TODO: Dev next.
                //Console.WriteLine("    radiate {id/name} = Highlight the OAPP for the given {id} or {name} in the STARNET Store. *Admin/Wizards Only*");
                //Console.WriteLine("    emit {id/name} = Show how much light the OAPP is emitting into the solar system for the given {id} or {name} (this is determined by the collective karma score of all users of that OAPP).");
                //Console.WriteLine("    reflect {id/name} = Show stats of the OAPP for the given {id} or {name}.");
                //Console.WriteLine("    evolve {id/name} = Upgrade/update a OAPP) for the given {id} or {name}."); //TODO: Dev next.
                //Console.WriteLine("    mutate {id/name} = Import/Export hApp, dApp & others for the given {id} or {name}.");
                //Console.WriteLine("    love {id/name} = Send/Receive Love for the given {id} or {name}.");
                //Console.WriteLine("    burst = View network stats/management/settings.");
                //Console.WriteLine("    super = Reserved For Future Use...");
                ////Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's, zomes, holons, celestial spaces, celestial bodies, geo-nft's, geo-hotspots, missions, chapters, quests & inventory items.");
                //Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's & more!");

                //Console.WriteLine("    avatar beamin = Beam in (log in).");
                //Console.WriteLine("    avatar beamout = Beam out (Log out).");
                //Console.WriteLine("    avatar whoisbeamedin = Display who is currently beamed in (if any) and the last time they beamed in and out.");
                //Console.WriteLine("    avatar show me = Display the currently beamed in avatar details (if any).");
                //Console.WriteLine("    avatar show {id/username} = Shows the details for the avatar for the given {id} or {username}.");
                //Console.WriteLine("    avatar edit = Edit the currently beamed in avatar.");
                //Console.WriteLine("    avatar list = Loads all avatars.");
                //Console.WriteLine("    avatar search = Seach avatars that match the given seach parameters (public fields only such as level, karma, username & any fields the player has set to public).");
                //Console.WriteLine("    karma list = Display the karma thresholds.");
                //Console.WriteLine("    keys link = Links a OASIS Provider Key to the current beamed in avatar.");
                //Console.WriteLine("    keys list = Shows the keys for the current beamed in avatar.");
                //Console.WriteLine("    wallet list = Shows the wallets for the current beamed in avatar.");
                //Console.WriteLine("    search = Seaches The OASIS for the given seach parameters.");
                //Console.WriteLine("    oapp create = Shortcut to the light sub-command.");
                //Console.WriteLine("    oapp update {id/name} = Update an existing OAPP for the given {id} or {name}.");
                //Console.WriteLine("    oapp delete {id/name} = Delete an existing OAPP for the given {id} or {name}.");
                //Console.WriteLine("    oapp install = Install a OAPP.");
                //Console.WriteLine("    oapp uninstall = Uninstall a OAPP.");
                //Console.WriteLine("    oapp publish {id/name} = Shortcut to the seed sub-command.");
                //Console.WriteLine("    oapp unpublish {id/name} = Shortcut to the un-seed sub-command.");
                //Console.WriteLine("    oapp show {id/name} = Shows a OAPP for the given {id} or {name}.");
                //Console.WriteLine("    oapp list [all] = List all OAPPs (contains zomes and holons) that have been generated.");
                //Console.WriteLine("    oapp list installed = List all OAPP's installed for the current beamed in avatar.");
                //Console.WriteLine("    oapp search [all] = Searches the OAPP's for the given search critera.");
                //Console.WriteLine("    happ create = Shortcut to the light sub-command.");
                //Console.WriteLine("    happ update {id/name} = Update an existing hApp for the given {id} or {name}.");
                //Console.WriteLine("    happ delete {id/name} = Delete an existing hApp for the given {id} or {name}.");
                //Console.WriteLine("    happ publish {id/name} = Shortcut to the seed sub-command.");
                //Console.WriteLine("    happ unpublish {id/name} = Shortcut to the un-seed sub-command.");
                //Console.WriteLine("    happ show {id/name} = Shows a hApp for the given {id} or {name}.");
                //Console.WriteLine("    happ list [all] = List all hApps (contains zomes) that have been generated.");
                //Console.WriteLine("    happ list installed = List all hApp's installed for the current beamed in avatar.");
                //Console.WriteLine("    happ search [all] = Searches the hApp's for the given search critera.");
                //Console.WriteLine("    zome create = Create a zome (module).");
                //Console.WriteLine("    zome update {id/name} = Update an existing zome for the given {id} or {name} (can upload a zome.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                //Console.WriteLine("    zome delete {id/name} = Delete an existing zome for the given {id} or {name}.");
                //Console.WriteLine("    zome publish {id/name} = Publishes a zome for the given {id} or {name} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                //Console.WriteLine("    zome unpublish {id/name} = Unpublishes a zome for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    zome show {id/name} = Shows a zome for the given {id} or {name}.");
                //Console.WriteLine("    zome list [all] = List all zomes (modules that contain holons) that have been generated.");
                //Console.WriteLine("    zome search [all] = Searches the zomes (modules) for the given search critera. If [all] is omitted it will search only your zomes otherwise it will search all public/shared zomes.");
                ////Console.WriteLine("    holon = Shows more info on how to use this command and optionally lauches the Save Holon Wizard.");
                //Console.WriteLine("    holon create json={holonJSONFile} = Creates/Saves a holon from the given {holonJSONFile}.");
                //Console.WriteLine("    holon create wiz = Starts the Create Holon Wizard.");
                //Console.WriteLine("    holon update {id/name} = Update an existing holon for the given {id} or {name} (can upload a holon.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                //Console.WriteLine("    holon delete {id/name} = Deletes a holon for the given {id} or {name}.");
                //Console.WriteLine("    holon publish {id/name} = Publishes a holon for the given {id} or {name} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                //Console.WriteLine("    holon unpublish {id/name} = Unpublishes a holon for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    holon show {id/name} = Shows a holon for the given {id} or {name}.");
                //Console.WriteLine("    holon list [all] = List all holons (OASIS Data Objects) that have been generated.");
                //Console.WriteLine("    holon search [all] = Searches the holons for the given search critera.");
                //Console.WriteLine("    celestialbody create = Creates a celestial body.");
                //Console.WriteLine("    celestialbody update {id/name} = Update an existing celestial body for the given {id} or {name}.");
                //Console.WriteLine("    celestialbody delete {id/name} = Delete an existing celestial body for the given {id} or {name}.");
                //Console.WriteLine("    celestialbody publish {id/name} = Publishes a celestial body for the given {id} or {name} to the STARNET store so others can use in their own OAPP's etc.");
                //Console.WriteLine("    celestialbody unpublish {id/name} = Unpublishes a celestial body for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    celestialbody show {id/name} = Shows a celestial body for the given {id} or {name}.");
                //Console.WriteLine("    celestialbody list [all] = List all celestial bodies that have been generated.");
                //Console.WriteLine("    celestialbody search [all] = Searches the celestial bodies for the given search critera.");
                //Console.WriteLine("    celestialspace create = Creates a celestial space.");
                //Console.WriteLine("    celestialspace update {id/name} = Update an existing celestial space for the given {id} or {name}.");
                //Console.WriteLine("    celestialspace delete {id/name} = Delete an existing celestial space for the given {id} or {name}.");
                //Console.WriteLine("    celestialspace publish {id/name} = Publishes a celestial space for the given {id} or {name} to the STARNET store so others can use in their own OAPP's etc.");
                //Console.WriteLine("    celestialspace unpublish {id/name} = Unpublishes a celestial space for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    celestialspace show {id/name} = Shows a celestial space for the given {id} or {name}.");
                //Console.WriteLine("    celestialspace list [all] = List all celestial spaces that have been generated.");
                //Console.WriteLine("    celestialspace search [all] = Searches the celestial spaces for the given search critera.");
                //Console.WriteLine("    nft mint = Mints a OASIS NFT for the current beamed in avatar.");
                //Console.WriteLine("    nft update {id/name} = Updates a nft for the given {id} or {name}.");
                //Console.WriteLine("    nft burn {id/name} = Burn's a nft for the given {id} or {name}.");
                //Console.WriteLine("    nft send {id/name} = Send a NFT for the given {id} or {name} to another wallet cross-chain.");
                //Console.WriteLine("    nft publish {id/name} = Publishes a OASIS NFT for the given {id} or {name} to the STARNET store so others can use in their own geo-nft's etc.");
                //Console.WriteLine("    nft unpublish {id/name} = Unpublishes a OASIS NFT for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    nft show {id/name} = Shows the NFT for the given {id} or {name}.");
                //Console.WriteLine("    nft list [all] = Shows the NFT's that belong to the current beamed in avatar.");
                //Console.WriteLine("    nft search [all] = Search for NFT's that match certain criteria and belong to the current beamed in avatar.");
                //Console.WriteLine("    geonft mint = Mints a OASIS Geo-NFT and places in Our World/AR World for the current beamed in avatar.");
                //Console.WriteLine("    geonft update {id/name} = Updates a geo-nft for the given {id} or {name}.");
                //Console.WriteLine("    geonft burn {id/name} = Burn's a geo-nft for the given {id} or {name}.");
                //Console.WriteLine("    geonft place {id/name} = Places an existing OASIS NFT for the given {id} or {name} in Our World/AR World for the current beamed in avatar.");
                //Console.WriteLine("    geonft send {id/name} = Send a geo-nft for the given {id} or {name} to another wallet cross-chain.");
                //Console.WriteLine("    geonft publish {id/name} = Publishes a geo-nft for the given {id} or {name} to the STARNET store so others can use in their own quests etc.");
                //Console.WriteLine("    geonft unpublish {id/name} = Unpublishes a geo-nft for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    geonft show {id/name} = Shows the Geo-NFT for the given {id} or {name}");
                //Console.WriteLine("    geonft list [all] = List all geo-nft's that have been created. If [all] is omitted it will list only your geo-nft's otherwise it will list all published geo-nft's as well as yours.");
                //Console.WriteLine("    geonft search [all] = Search for Geo-NFT's that match certain criteria and belong to the current beamed in avatar. If [all] is used then it will also include any shared/public/published geo-nft's");
                //Console.WriteLine("    inventoryitem create = Creates an inventory item that can be granted as a reward (will be placed in the avatar's inventory) for completing quests, collecting geo-nft's, triggering geo-hotspots etc.");
                //Console.WriteLine("    inventoryitem update {id/name} = Updates a inventory item for the given {id} or {name}.");
                //Console.WriteLine("    inventoryitem delete {id/name} = Deletes a inventory item for the given {id} or {name}.");
                //Console.WriteLine("    inventoryitem publish {id/name} = Publishes an inventory item for the given {id} or {name} to the STARNET store so others can use in their own quests, geo-hotspots, geo-nfts, etc.");
                //Console.WriteLine("    inventoryitem unpublish {id/name} = Unpublishes an inventory item  for the given {id} or {name} from the STARNET store.");
                //Console.WriteLine("    inventoryitem show {id/name} = Shows the inventory item for the given {id} or {name}.");
                //Console.WriteLine("    inventoryitem list [all] = List all inventory item's that have been created.");
                //Console.WriteLine("    inventoryitem search [all] = Search all inventory item's that have been created.");
                ////Console.WriteLine("    inventoryitem activate = Activates an inventory item that has been published to the STARNET store so is visible to others.");
                ////Console.WriteLine("    inventoryitem deactivate = Deactivates an inventory item that has been published to the STARNET store so is invisible to others.");
                //Console.WriteLine("    geohotspot create = Creates a geo-hotspot that chapters & quests can be added to.");
                //Console.WriteLine("    geohotspot update {id/name} = Updates a geo-hotspot for the given {id} or {name}.");
                //Console.WriteLine("    geohotspot delete {id/name} = Deletes an geo-hotspot for the given {id} or {name}.");
                //Console.WriteLine("    geohotspot publish {id/name} = Publishes a geo-hotspot for the given {id} or {name} to the STARNET store so others can use in their own quests.");
                //Console.WriteLine("    geohotspot unpublish {id/name} = Unpublishes a geo-hotspot from the STARNET store.");
                //Console.WriteLine("    geohotspot show {id/name} = Shows the geo-hotspot for the given {id} or {name}.");
                //Console.WriteLine("    geohotspots list [all] = List all geo-hotspot's that have been created.");
                //Console.WriteLine("    geohotspots search [all] = Search all geo-hotspot's that have been created.");
                //Console.WriteLine("    mission create = Creates a mission that chapters & quests can be added to.");
                //Console.WriteLine("    mission update {id/name} = Updates a mission for the given {id} or {name}.");
                //Console.WriteLine("    mission delete {id/name} = Deletes an mission for the given {id} or {name}.");
                //Console.WriteLine("    mission publish {id/name} = Publishes a mission  for the given {id} or {name} to the STARNET store so others can find and play in Our World/AR World, One World & any other OASIS OAPP.");
                //Console.WriteLine("    mission unpublish {id/name} = Unpublishes a mission from the STARNET store for the given {id} or {name}.");
                //Console.WriteLine("    mission show {id/name} = Shows the mission for the given {id} or {name}.");
                //Console.WriteLine("    missions list [all] = List all mission's that have been created.");
                //Console.WriteLine("    missions search [all] = Search all mission's that have been created.");
                //Console.WriteLine("    quest create = Creates a quest that can be linked to a mission. Geo-nfts, geo-hotspots & rewards can be linked to the quest.");
                //Console.WriteLine("    quest update {id/name} = Updates a quest for the given {id} or {name}.");
                //Console.WriteLine("    quest delete {id/name} = Deletes a quest for the given {id} or {name}.");
                //Console.WriteLine("    quest publish {id/name} = Publishes a quest to the STARNET store so others can use in their own quests as sub-quests or in missions/chapters.");
                //Console.WriteLine("    quest unpublish {id/name} = Unpublishes a quest from the STARNET store for the given {id} or {name}.");
                //Console.WriteLine("    quest show {id/name} = Shows the quest for the given {id} or {name}.");
                //Console.WriteLine("    quest list [all] = List all quests that have been created.");
                //Console.WriteLine("    quest search [all] - Search all quests that have been created.");
                //Console.WriteLine("    chapter create = Creates a chapter that can be linked to a mission. Quests can be added to the chapter. Chapters are used to group quests together (optional).");
                //Console.WriteLine("    chapter update {id/name} = Updates a chapter for the given {id} or {name}.");
                //Console.WriteLine("    chapter delete {id/name} = Deletes a chapter for the given {id} or {name}.");
                //Console.WriteLine("    chapter publish {id/name} = Publishes a chapter to the STARNET store for the given {id} or {name} so others can use in their own missions.");
                //Console.WriteLine("    chapter unpublish {id/name} = Unpublishes a chapter from the STARNET store for the given {id} or {name}.");
                //Console.WriteLine("    chapter show {id/name} = Shows the chapter for the given {id} or {name}.");
                //Console.WriteLine("    chapter list [all] = List chapters that have been created.");
                //Console.WriteLine("    chapter search [all] = Search chapters that have been created.");
                //Console.WriteLine("    onode start = Starts a OASIS Node (ONODE) and registers it on the OASIS Network (ONET).");
                //Console.WriteLine("    onode stop = Stops a OASIS Node (ONODE).");
                //Console.WriteLine("    onode status = Shows stats for this ONODE.");
                //Console.WriteLine("    onode config = Opens the ONODE's OASISDNA to allow changes to be made (you will need to stop and start the ONODE for changes to apply).");
                //Console.WriteLine("    onode providers = Shows what OASIS Providers are running for this ONODE.");
                //Console.WriteLine("    onode startprovider {ProviderName} = Starts a given provider.");
                //Console.WriteLine("    onode stopprovider {ProviderName} = Stops a given provider.");
                //Console.WriteLine("    hypernet start = Starts the HoloNET P2P HyperNET Service.");
                //Console.WriteLine("    hypernet stop = Stops the HoloNET P2P HyperNET Service.");
                //Console.WriteLine("    hypernet status = Shows stats for the HoloNET P2P HyperNET Service.");
                //Console.WriteLine("    hypernet config = Opens the HyperNET's DNA to allow changes to be made (you will need to stop and start the HyperNET Service for changes to apply.");
                //Console.WriteLine("    onet status = Shows stats for the OASIS Network (ONET).");
                //Console.WriteLine("    onet providers                                           Shows what OASIS Providers are running across the ONET and on what ONODE's.");
                //Console.WriteLine("    config cosmicdetailedoutput {enable/disable/status}      Enables/disables COSMIC Detailed Output.");
                //Console.WriteLine("    config starstatusdetailedoutput {enable/disable/status}  Enables/disables STAR ODK Detailed Output.");
                //Console.WriteLine("    runcosmictests    {OAPPType} {dnaFolder} {geneisFolder}  Run the STAR ODK/COSMIC Tests... If OAPPType, DNAFolder or GenesisFolder are not specified it will use the defaults.");
                //Console.WriteLine("    runoasisapitests                                         Run the OASIS API Tests...");
            }
            else
            {
                Console.WriteLine("    light            Generate a OAPP.");
                Console.WriteLine("    bang             Generate a whole metaverse or part of one such as Multierveres, Universes, Dimensions, Galaxy Clusters, Galaxies, Solar Systems, Stars, Planets, Moons etc.");
                Console.WriteLine("    wiz              Start the STAR ODK Wizard which will walk you through the steps for creating a OAPP tailored to your specefic needs (such as which OASIS Providers do you need and the specefic use case(s) you need etc).");
                Console.WriteLine("    flare            Build a OAPP.");
                Console.WriteLine("    shine            Launch & activate a OAPP by shining the 's light upon it..."); //TODO: Dev next.
                Console.WriteLine("    twinkle          Activate a published OAPP within the STARNET store."); //TODO: Dev next.
                Console.WriteLine("    dim              Deactivate a published OAPP within the STARNET store."); //TODO: Dev next.
                Console.WriteLine("    seed             Deploy/Publish a OAPP to the STARNET Store.");
                Console.WriteLine("    unseed           Undeploy/Unpublish a OAPP from the STARNET Store.");
                Console.WriteLine("    dust             Delete a OAPP (this will also remove it from STARNET if it has already been published)."); //TODO: Dev next.
                Console.WriteLine("    radiate          Highlight the OAPP in the STARNET Store. *Admin/Wizards Only*");
                Console.WriteLine("    emit             Show how much light the OAPP is emitting into the solar system (this is determined by the collective karma score of all users of that OAPP).");
                Console.WriteLine("    reflect          Show stats of the OAPP.");
                Console.WriteLine("    evolve           Upgrade/update a OAPP)."); //TODO: Dev next.
                Console.WriteLine("    mutate           Import/Export hApp, dApp & others.");
                Console.WriteLine("    love             Send/Receive Love.");
                Console.WriteLine("    burst            View network stats/management/settings.");
                Console.WriteLine("    super            Reserved For Future Use...");
                //Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's, zomes, holons, celestial spaces, celestial bodies, geo-nft's, geo-hotspots, missions, chapters, quests & inventory items.");
                Console.WriteLine("    net              Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's & more!");
                Console.WriteLine("    avatar           Manage avatars.");
                Console.WriteLine("    karma            Manage karma.");
                Console.WriteLine("    keys             Manage keys.");
                Console.WriteLine("    wallet           Manage wallets.");
                Console.WriteLine("    search           Search the OASIS.");
                Console.WriteLine("    oapp             Create, edit, delete, publish, unpublish, install, uninstall, list & show OAPP's.");
                Console.WriteLine("    happ             Create, edit, delete, publish, unpublish, install, uninstall, list & show hApp's.");
                Console.WriteLine("    celestialspace   Create, edit, delete, publish, unpublish, list & show celestial space's.");
                Console.WriteLine("    celestialbody    Create, edit, delete, publish, unpublish, list & show celestial bodies's.");
                Console.WriteLine("    zome             Create, edit, delete, publish, unpublish, list & show zome's.");
                Console.WriteLine("    holon            Create, edit, delete, publish, unpublish, list & show holon's.");
                Console.WriteLine("    chapter          Create, edit, delete, publish, unpublish, list & show chapter's.");
                Console.WriteLine("    mission          Create, edit, delete, publish, unpublish, list & show mission's.");
                Console.WriteLine("    quest            Create, edit, delete, publish, unpublish, list & show quest's.");
                Console.WriteLine("    nft              Mint, send, edit, burn, publish, unpublish, list & show nft's.");
                Console.WriteLine("    geonft           Mint, place, edit, burn, publish, unpublish, list & show geo-nft's.");
                Console.WriteLine("    geohotspot       Create, edit, delete, publish, unpublish, list & show geo-hotspot's.");
                Console.WriteLine("    inventoryitem    Create, edit, delete, publish, unpublish, list & show inventory item's.");
                Console.WriteLine("    seeds            Access the SEEDS API.");
                Console.WriteLine("    data             Access the Data API.");
                Console.WriteLine("    map              Access the Map API.");
                Console.WriteLine("    oland            Access the OLAND (Virtual Land) API.");
                Console.WriteLine("    onode            Manage this ONODE (OASIS Node) such as start, stop, view status, edit config, view providers, start & stop providers.");
                Console.WriteLine("    hypernet         Start, stop & view status for the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    onet             View the status for the ONET (OASIS Network).");
                Console.WriteLine("    config           Enables/disables COSMIC detailed output & STAR ODK detailed output.");
                Console.WriteLine("    runcosmictests   Run the STAR ODK/COSMIC tests.");
                Console.WriteLine("    runoasisapitests Run the OASIS API tests.");
                Console.WriteLine("");
                //Console.WriteLine(" NOTES: -  is not needed if using the STAR CLI Console directly. Star is only needed if calling from the command line or another external script ( is simply the name of the exe).");
                Console.WriteLine(" NOTES:");
                Console.WriteLine("                     When invoking any sub-commands that take a {id} or {title}, if neither is specified then a wizard will launch to help find the correct item.");
                Console.WriteLine("                     When invoking any sub-commands that have an optional [all] argument/flag, if it is omitted it will search only your items, otherwise it will search all published items as well as yours.");
                Console.WriteLine("                     If you invoke a sub-command without any arguments it will show more detailed help on how to use that sub-command as well as the option to lanuch any wizards to help guide you.");
            }



            /*
            Console.WriteLine("    ignite = Ignite SuperStar & Boot The OASIS");
            Console.WriteLine("    extinguish = Extinguish SuperStar & Shutdown The OASIS");
            Console.WriteLine("    beamin = Log in");
            Console.WriteLine("    beamout = Log out");
            Console.WriteLine("    help {full} = Show this help page. If {full} is omitted it will show only the top level sub-commands, if [full} is included it will show every option for each sub-command.");

            if (showFullCommands)
            {
                Console.WriteLine("    light {OAPPName} {OAPPDesc} {OAPPType} {dnaFolder} {geneisFolder} {genesisNameSpace} {genesisTyp} {parentCelestialBodyId} (optional) = Creates a new OAPP (Zomes/Holons/Star/Planet/Moon) at the given genesis folder location, from the given OAPP DNA.");
                Console.WriteLine("    light = Displays more detail on how to use this command and optionally launches the Light Wizard.");
                Console.WriteLine("    light wiz = Start the Light Wizard.");
                Console.WriteLine("    light transmute {hAppDNA} {geneisFolder}  = Creates a new Planet (OApp) at the given folder genesis locations, from the given hApp DNA.");
            }
            else
                Console.WriteLine("    light = Show OAPP generation sub-command help.");

            Console.WriteLine("    bang = Generate a whole metaverse or part of one such as Multierveres, Universes, Dimensions, Galaxy Clusters, Galaxies, Solar Systems, Stars, Planets, Moons etc.");
            Console.WriteLine("    wiz = Start the STAR ODK Wizard which will walk you through the steps for creating a OAPP tailored to your specefic needs (such as which OASIS Providers do you need and the specefic use case(s) you need etc).");
            Console.WriteLine("    flare {OAPPName/OAPPId} = Build a OAPP.");
            Console.WriteLine("    shine {OAPPName/OAPPId} = Launch & activate a OAPP by shining the 's light upon it..."); //TODO: Dev next.
            Console.WriteLine("    twinkle {OAPPName/OAPPId} = Activate a published OAPP within the STARNET store."); //TODO: Dev next.
            Console.WriteLine("    dim {OAPPName/OAPPId} = Deactivate a published OAPP within the STARNET store."); //TODO: Dev next.
            Console.WriteLine("    seed {OAPPName/OAPPId} = Deploy/Publish a OAPP to the STARNET Store.");
            Console.WriteLine("    unseed {OAPPName/OAPPId} = Undeploy/Unpublish a OAPP from the STARNET Store.");
            Console.WriteLine("    dust {OAPPName/OAPPId} = Delete a OAPP (this will also remove it from STARNET if it has already been published)."); //TODO: Dev next.
            Console.WriteLine("    radiate {OAPPName/OAPPId} = Highlight the OAPP in the OAPP Store (StarNET). *Admin/Wizards Only*");
            Console.WriteLine("    emit {OAPPName/OAPPId} = Show how much light the OAPP is emitting into the solar system (this is determined by the collective karma score of all users of that OAPP).");
            Console.WriteLine("    reflect {OAPPName/OAPPId} = Show stats of the OAPP.");
            Console.WriteLine("    evolve {OAPPName/OAPPId} = Upgrade/update a OAPP)."); //TODO: Dev next.
            Console.WriteLine("    mutate {OAPPName/OAPPId} = Import/Export hApp, dApp & others.");
            Console.WriteLine("    love {OAPPName/OAPPId} = Send/Receive Love.");
            Console.WriteLine("    burst = View network stats/management/settings.");
            Console.WriteLine("    super = Reserved For Future Use...");
            //Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's, zomes, holons, celestial spaces, celestial bodies, geo-nft's, geo-hotspots, missions, chapters, quests & inventory items.");
            Console.WriteLine("    net = Launch the STARNET Library/Store where you can list, search, update, publish, unpublish, install & uninstall OAPP's & more!");

            if (showFullCommands)
            {
                Console.WriteLine("    avatar beamedin = Display who is currently beamed in (if any) and the last time they beamed in and out.");
                Console.WriteLine("    avatar show me = Display the currently beamed in avatar details (if any).");
                Console.WriteLine("    avatar show {id/username} = Shows the details for the avatar for the given {id} or {username}.");
                Console.WriteLine("    avatar edit = Edit the currently beamed in avatar.");
                Console.WriteLine("    avatar list = Loads all avatars.");
                Console.WriteLine("    avatar search = Seach avatars that match the given seach parameters (public fields only such as level, karma, username & any fields the player has set to public).");
                Console.WriteLine("    karma list = Display the karma thresholds.");
                Console.WriteLine("    keys link = Links a OASIS Provider Key to the current beamed in avatar.");
                Console.WriteLine("    keys list = Shows the keys for the current beamed in avatar.");
                Console.WriteLine("    wallet list = Shows the wallets for the current beamed in avatar.");
                Console.WriteLine("    search = Seaches The OASIS for the given seach parameters.");
                Console.WriteLine("    oapp create = Shortcut to the light sub-command.");
                Console.WriteLine("    oapp update {id/title} = Update an existing OAPP for the given {id} or {title}.");
                Console.WriteLine("    oapp delete {id/title} = Delete an existing OAPP for the given {id} or {title}.");
                Console.WriteLine("    oapp install = Install a OAPP.");
                Console.WriteLine("    oapp uninstall = Uninstall a OAPP.");
                Console.WriteLine("    oapp publish {id/title} = Shortcut to the seed sub-command.");
                Console.WriteLine("    oapp unpublish {id/title} = Shortcut to the un-seed sub-command.");
                Console.WriteLine("    oapp show {id/title} = Shows a OAPP for the given {id} or {title}.");
                Console.WriteLine("    oapp list {all} = List all OAPPs (contains zomes and holons) that have been generated.");
                Console.WriteLine("    oapp list installed = List all OAPP's installed for the current beamed in avatar.");
                Console.WriteLine("    oapp search {all} = Searches the OAPP's for the given search critera.");
                Console.WriteLine("    happ create = Shortcut to the light sub-command.");
                Console.WriteLine("    happ update {id/title} = Update an existing hApp for the given {id} or {title}.");
                Console.WriteLine("    happ delete {id/title} = Delete an existing hApp for the given {id} or {title}.");
                Console.WriteLine("    happ publish {id/title} = Shortcut to the seed sub-command.");
                Console.WriteLine("    happ unpublish {id/title} = Shortcut to the un-seed sub-command.");
                Console.WriteLine("    happ show {id/title} = Shows a hApp for the given {id} or {title}.");
                Console.WriteLine("    happ list {all} = List all hApps (contains zomes) that have been generated.");
                Console.WriteLine("    happ list installed = List all hApp's installed for the current beamed in avatar.");
                Console.WriteLine("    happ search {all} = Searches the hApp's for the given search critera.");
                Console.WriteLine("    zome create = Create a zome (module).");
                Console.WriteLine("    zome update {id/title} = Update an existing zome for the given {id} or {title} (can upload a zome.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                Console.WriteLine("    zome delete {id/title} = Delete an existing zome for the given {id} or {title}.");
                Console.WriteLine("    zome publish {id/title} = Publishes a zome for the given {id} or {title} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                Console.WriteLine("    zome unpublish {id/title} = Unpublishes a zome for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    zome show {id/title} = Shows a zome for the given {id} or {title}.");
                Console.WriteLine("    zome list {all} = List all zomes (modules that contain holons) that have been generated.");
                Console.WriteLine("    zome search {all} = Searches the zomes (modules) for the given search critera. If {all} is omitted it will search only your zomes otherwise it will search all public/shared zomes.");
                //Console.WriteLine("    holon = Shows more info on how to use this command and optionally lauches the Save Holon Wizard.");
                Console.WriteLine("    holon create json={holonJSONFile} = Creates/Saves a holon from the given {holonJSONFile}.");
                Console.WriteLine("    holon create wiz = Starts the Create Holon Wizard.");
                Console.WriteLine("    holon update {id/title} = Update an existing holon for the given {id} or {title} (can upload a holon.cs file containing custom code/logic/functions which is then shareable with other OAPP's).");
                Console.WriteLine("    holon delete {id/title} = Deletes a holon for the given {id} or {title}.");
                Console.WriteLine("    holon publish {id/title} = Publishes a holon for the given {id} or {title} to the STARNET store so others can use in their own OAPP's/hApp's etc.");
                Console.WriteLine("    holon unpublish {id/title} = Unpublishes a holon for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    holon show {id/title} = Shows a holon for the given {id} or {title}.");
                Console.WriteLine("    holon list {all} = List all holons (OASIS Data Objects) that have been generated.");
                Console.WriteLine("    holon search {all} = Searches the holons for the given search critera.");
                Console.WriteLine("    celestialbody create = Creates a celestial body.");
                Console.WriteLine("    celestialbody update {id/title} = Update an existing celestial body for the given {id} or {title}.");
                Console.WriteLine("    celestialbody delete {id/title} = Delete an existing celestial body for the given {id} or {title}.");
                Console.WriteLine("    celestialbody publish {id/title} = Publishes a celestial body for the given {id} or {title} to the STARNET store so others can use in their own OAPP's etc.");
                Console.WriteLine("    celestialbody unpublish {id/title} = Unpublishes a celestial body for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    celestialbody show {id/title} = Shows a celestial body for the given {id} or {title}.");
                Console.WriteLine("    celestialbody list {all} = List all celestial bodies that have been generated.");
                Console.WriteLine("    celestialbody search {all} = Searches the celestial bodies for the given search critera.");
                Console.WriteLine("    celestialspace create = Creates a celestial space.");
                Console.WriteLine("    celestialspace update {id/title} = Update an existing celestial space for the given {id} or {title}.");
                Console.WriteLine("    celestialspace delete {id/title} = Delete an existing celestial space for the given {id} or {title}.");
                Console.WriteLine("    celestialspace publish {id/title} = Publishes a celestial space for the given {id} or {title} to the STARNET store so others can use in their own OAPP's etc.");
                Console.WriteLine("    celestialspace unpublish {id/title} = Unpublishes a celestial space for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    celestialspace show {id/title} = Shows a celestial space for the given {id} or {title}.");
                Console.WriteLine("    celestialspace list {all} = List all celestial spaces that have been generated.");
                Console.WriteLine("    celestialspace search {all} = Searches the celestial spaces for the given search critera.");
                Console.WriteLine("    nft mint = Mints a OASIS NFT for the current beamed in avatar.");
                Console.WriteLine("    nft send {id/title} = Send a NFT for the given {id} or {title} to another wallet cross-chain.");
                Console.WriteLine("    nft publish {id/title} = Publishes a OASIS NFT for the given {id} or {title} to the STARNET store so others can use in their own geo-nft's etc.");
                Console.WriteLine("    nft unpublish {id/title} = Unpublishes a OASIS NFT for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    nft show {id/title} = Shows the NFT for the given {id} or {title}.");
                Console.WriteLine("    nft list {all} = Shows the NFT's that belong to the current beamed in avatar.");
                Console.WriteLine("    nft search {all} = Search for NFT's that match certain criteria and belong to the current beamed in avatar.");
                Console.WriteLine("    geonft mint = Mints a OASIS Geo-NFT and places in Our World/AR World for the current beamed in avatar.");
                Console.WriteLine("    geonft place {id/title} = Places an existing OASIS NFT for the given {id} or {title} in Our World/AR World for the current beamed in avatar.");
                Console.WriteLine("    geonft send {id/title} = Send a geo-nft for the given {id} or {title} to another wallet cross-chain.");
                Console.WriteLine("    geonft publish {id/title} = Publishes a geo-nft for the given {id} or {title} to the STARNET store so others can use in their own quests etc.");
                Console.WriteLine("    geonft unpublish {id/title} = Unpublishes a geo-nft for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    geonft show {id/title} = Shows the Geo-NFT for the given {id} or {title}");
                Console.WriteLine("    geonft list {all} = List all geo-nft's that have been created. If {all} is omitted it will list only your geo-nft's otherwise it will list all published geo-nft's as well as yours.");
                Console.WriteLine("    geonft search {all} = Search for Geo-NFT's that match certain criteria and belong to the current beamed in avatar. If {all} is used then it will also include any shared/public/published geo-nft's");
                Console.WriteLine("    inventoryitem create = Creates an inventory item that can be granted as a reward (will be placed in the avatar's inventory) for completing quests, collecting geo-nft's, triggering geo-hotspots etc.");
                Console.WriteLine("    inventoryitem update {id/title} = Updates a inventory item for the given {id} or {title}.");
                Console.WriteLine("    inventoryitem delete {id/title} = Deletes a inventory item for the given {id} or {title}.");
                Console.WriteLine("    inventoryitem publish {id/title} = Publishes an inventory item for the given {id} or {title} to the STARNET store so others can use in their own quests, geo-hotspots, geo-nfts, etc.");
                Console.WriteLine("    inventoryitem unpublish {id/title} = Unpublishes an inventory item  for the given {id} or {title} from the STARNET store.");
                Console.WriteLine("    inventoryitem show {id/title} = Shows the inventory item for the given {id} or {title}.");
                Console.WriteLine("    inventoryitem list {all} = List all inventory item's that have been created.");
                Console.WriteLine("    inventoryitem search {all} = Search all inventory item's that have been created.");
                //Console.WriteLine("    inventoryitem activate = Activates an inventory item that has been published to the STARNET store so is visible to others.");
                //Console.WriteLine("    inventoryitem deactivate = Deactivates an inventory item that has been published to the STARNET store so is invisible to others.");
                Console.WriteLine("    geohotspot create = Creates a geo-hotspot that chapters & quests can be added to.");
                Console.WriteLine("    geohotspot update {id/title} = Updates a geo-hotspot for the given {id} or {title}.");
                Console.WriteLine("    geohotspot delete {id/title} = Deletes an geo-hotspot for the given {id} or {title}.");
                Console.WriteLine("    geohotspot publish {id/title} = Publishes a geo-hotspot for the given {id} or {title} to the STARNET store so others can use in their own quests.");
                Console.WriteLine("    geohotspot unpublish {id/title} = Unpublishes a geo-hotspot from the STARNET store.");
                Console.WriteLine("    geohotspot show {id/title} = Shows the geo-hotspot for the given {id} or {title}.");
                Console.WriteLine("    geohotspots list {all} = List all geo-hotspot's that have been created.");
                Console.WriteLine("    geohotspots search {all} = Search all geo-hotspot's that have been created.");
                Console.WriteLine("    mission create = Creates a mission that chapters & quests can be added to.");
                Console.WriteLine("    mission update {id/title} = Updates a mission for the given {id} or {title}.");
                Console.WriteLine("    mission delete {id/title} = Deletes an mission for the given {id} or {title}.");
                Console.WriteLine("    mission publish {id/title} = Publishes a mission  for the given {id} or {title} to the STARNET store so others can find and play in Our World/AR World, One World & any other OASIS OAPP.");
                Console.WriteLine("    mission unpublish {id/title} = Unpublishes a mission from the STARNET store for the given {id} or {title}.");
                Console.WriteLine("    mission show {id/title} = Shows the mission for the given {id} or {title}.");
                Console.WriteLine("    missions list {all} = List all mission's that have been created.");
                Console.WriteLine("    missions search {all} = Search all mission's that have been created.");
                Console.WriteLine("    quest create = Creates a quest that can be linked to a mission. Geo-nfts, geo-hotspots & rewards can be linked to the quest.");
                Console.WriteLine("    quest update {id/title} = Updates a quest for the given {id} or {title}.");
                Console.WriteLine("    quest delete {id/title} = Deletes a quest for the given {id} or {title}.");
                Console.WriteLine("    quest publish {id/title} = Publishes a quest to the STARNET store so others can use in their own quests as sub-quests or in missions/chapters.");
                Console.WriteLine("    quest unpublish {id/title} = Unpublishes a quest from the STARNET store for the given {id} or {title}.");
                Console.WriteLine("    quest show {id/title} = Shows the quest for the given {id} or {title}.");
                Console.WriteLine("    quest list {all} = List all quests that have been created.");
                Console.WriteLine("    quest search {all} - Search all quests that have been created.");
                Console.WriteLine("    chapter create = Creates a chapter that can be linked to a mission. Quests can be added to the chapter. Chapters are used to group quests together (optional).");
                Console.WriteLine("    chapter update {id/title} = Updates a chapter for the given {id} or {title}.");
                Console.WriteLine("    chapter delete {id/title} = Deletes a chapter for the given {id} or {title}.");
                Console.WriteLine("    chapter publish {id/title} = Publishes a chapter to the STARNET store for the given {id} or {title} so others can use in their own missions.");
                Console.WriteLine("    chapter unpublish {id/title} = Unpublishes a chapter from the STARNET store for the given {id} or {title}.");
                Console.WriteLine("    chapter show {id/title} = Shows the chapter for the given {id} or {title}.");
                Console.WriteLine("    chapter list {all} = List chapters that have been created.");
                Console.WriteLine("    chapter search {all} = Search chapters that have been created.");
                Console.WriteLine("    onode start = Starts a OASIS Node (ONODE) and registers it on the OASIS Network (ONET).");
                Console.WriteLine("    onode stop = Stops a OASIS Node (ONODE).");
                Console.WriteLine("    onode status = Shows stats for this ONODE.");
                Console.WriteLine("    onode config = Opens the ONODE's OASISDNA to allow changes to be made (you will need to stop and start the ONODE for changes to apply).");
                Console.WriteLine("    onode providers = Shows what OASIS Providers are running for this ONODE.");
                Console.WriteLine("    onode startprovider {ProviderName} = Starts a given provider.");
                Console.WriteLine("    onode stopprovider {ProviderName} = Stops a given provider.");
                Console.WriteLine("    hypernet start = Starts the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    hypernet stop = Stops the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    hypernet status = Shows stats for the HoloNET P2P HyperNET Service.");
                Console.WriteLine("    onet status = Shows stats for the OASIS Network (ONET).");
                Console.WriteLine("    onet providers = Shows what OASIS Providers are running across the ONET and on what ONODE's.");
            }
            else
            {
                Console.WriteLine("    avatar = Show avatar sub-command help.");
                Console.WriteLine("    karma = Show karma sub-command help.");
                Console.WriteLine("    keys = Show keys sub-command help.");
                Console.WriteLine("    wallet = Show wallet sub-command help.");
                Console.WriteLine("    search = Show search sub-command help.");
                Console.WriteLine("    oapp = Show oapp sub-command help.");
                Console.WriteLine("    happ = Show happ sub-command help.");
                Console.WriteLine("    zome = Show zome sub-command help.");
                //Console.WriteLine("    holon = Shows more info on how to use this command and optionally lauches the Save Holon Wizard.");
                Console.WriteLine("    holon = Show holon sub-command help.");
                Console.WriteLine("    celestialbody = Show celestialbody sub-command help.");
                Console.WriteLine("    celestialspace = Show celestialspace sub-command help.");
                Console.WriteLine("    nft = Show nft sub-command help.");
                Console.WriteLine("    geonft = Show geonft sub-command help."); ;
                Console.WriteLine("    inventoryitem = Show inventoryitem sub-command help.");
                Console.WriteLine("    geohotspot = Show geohotspot sub-command help.");
                Console.WriteLine("    mission = Show mission sub-command help.");
                Console.WriteLine("    quest = Show quest sub-command help.");
                Console.WriteLine("    chapter = Show chapter sub-command help.");
                Console.WriteLine("    onode = Show onode sub-command help.");
                Console.WriteLine("    hypernet = Show hypernet sub-command help.");
                Console.WriteLine("    onet = Show onet sub-command help.");
            }

            Console.WriteLine("    version = Show the versions of STAR ODK, COSMIC ORM, OASIS Runtime & the OASIS Providers..");
            Console.WriteLine("    status = Show the status of STAR ODK.");
            Console.WriteLine("    exit = Exit the STAR CLI.");
            Console.WriteLine("    cosmicdetailedoutput enable = Enables COSMIC Detailed Output.");
            Console.WriteLine("    cosmicdetailedoutput disable = Disables COSMIC Detailed Output.");
            Console.WriteLine("    starstatusdetailedoutput enable = Enables STAR ODK Detailed Output.");
            Console.WriteLine("    starstatusdetailedoutput disable = Disables STAR ODK Detailed Output.");
            Console.WriteLine("    runcosmictests {OAPPType} {dnaFolder} {geneisFolder} = Run the STAR ODK/COSMIC Tests... If OAPPType, DNAFolder or GenesisFolder are not specified it will use the defaults.");
            Console.WriteLine("    runoasisapitests = Run the OASIS API Tests...");
            */


            // Console.Ge
            Console.WriteLine("************************************************************************************************");
            //Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;
        }

        //private static async Task LoginAvatar()
        //{
        //    OASISResult<IAvatar> beamInResult = null;

        //    while (beamInResult == null || (beamInResult != null && beamInResult.IsError))
        //    {
        //        if (!CLIEngine.GetConfirmation("Do you have an existing avatar? "))
        //            CreateAvatar();
        //        else
        //            CLIEngine.ShowMessage("", false);

        //        CLIEngine.ShowMessage("Please login below:");
        //        //string username = GetValidEmail("Username? ", false);
        //        string username = GetValidUsername("Username? ", false);
        //        string password = CLIEngine.ReadPassword("Password? ");
        //        CLIEngine.ShowWorkingMessage("Beaming In...");

        //        CLIEngine.SupressConsoleLogging = true;
        //        beamInResult = Task.Run(async () => await STAR.BeamInAsync(username, password)).Result;
        //        CLIEngine.SupressConsoleLogging = false;

        //        //CLIEngine.ShowWorkingMessage("Beaming In...");
        //        //beamInResult = Task.Run(async () => await STAR.BeamInAsync("davidellams@hotmail.com", "my-super-secret-password")).Result;
        //        //beamInResult = Task.Run(async () => await STAR.BeamInAsync("davidellams@hotmail.com", "new-super-secret-password")).Result;
        //        //beamInResult = Task.Run(async () => await STAR.BeamInAsync("davidellams@hotmail.com", "test!")).Result;

        //        //beamInResult = STAR.BeamIn("davidellams@hotmail.com", "my-super-secret-password");
        //        //beamInResult = STAR.BeamIn("davidellams@hotmail.com", "test!");
        //        //beamInResult = STAR.BeamIn("davidellams@gmail.com", "test!");

        //        CLIEngine.ShowMessage("");

        //        if (beamInResult.IsError)
        //        {
        //            CLIEngine.ShowErrorMessage(string.Concat("Error Beaming in. Error Message: ", beamInResult.Message));

        //            if (beamInResult.Message == "Avatar has not been verified. Please check your email.")
        //            {
        //                CLIEngine.ShowErrorMessage("Then either click the link in the email to activate your avatar or enter the validation token contained in the email below:", false);

        //                bool validToken = false;
        //                while (!validToken)
        //                {
        //                    string token = CLIEngine.GetValidInput("Enter validation token: ");
        //                    CLIEngine.ShowWorkingMessage("Verifying Token...");
        //                    OASISResult<bool> verifyEmailResult = STAR.OASISAPI.Avatar.VerifyEmail(token);

        //                    if (verifyEmailResult.IsError)
        //                        CLIEngine.ShowErrorMessage(verifyEmailResult.Message);
        //                    else
        //                    {
        //                        CLIEngine.ShowSuccessMessage("Verification successful, you can now login");
        //                        validToken = true;
        //                    }
        //                }
        //            }
        //        }

        //        else if (STAR.LoggedInAvatar == null)
        //            CLIEngine.ShowErrorMessage("Error Beaming In. Username/Password may be incorrect.");
        //    }

        //    CLIEngine.ShowSuccessMessage(string.Concat("Successfully Beamed In! Welcome back ", STAR.LoggedInAvatar.Username, ". Have a nice day! :) You Are Level ", STAR.LoggedInAvatarDetail.Level, " And Have ", STAR.LoggedInAvatarDetail.Karma, " Karma."));
        //    //ShowAvatarStats();
        //    await ReadyPlayerOne();
        //}

        //private static void ShowAvatarStats()
        //{
        //    ShowAvatarStats(STAR.LoggedInAvatar, STAR.LoggedInAvatarDetail);
        //}

        //private static void ShowAvatarStats(IAvatar avatar, IAvatarDetail avatarDetail)
        //{
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    CLIEngine.ShowMessage("", false);
        //    CLIEngine.ShowMessage($"Avatar {avatar.Username} Beamed In On {avatar.LastBeamedIn} And Last Beamed Out On {avatar.LastBeamedOut}.");
        //    Console.WriteLine("");
        //    Console.WriteLine(string.Concat(" Name: ", avatar.FullName));
        //    Console.WriteLine(string.Concat(" Created: ", avatar.CreatedDate));
        //    Console.WriteLine(string.Concat(" Karma: ", avatarDetail.Karma));
        //    Console.WriteLine(string.Concat(" Level: ", avatarDetail.Level));
        //    Console.WriteLine(string.Concat(" XP: ", avatarDetail.XP));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Chakras:");
        //    Console.WriteLine(string.Concat(" Crown XP: ", avatarDetail.Chakras.Crown.XP));
        //    Console.WriteLine(string.Concat(" Crown Level: ", avatarDetail.Chakras.Crown.Level));
        //    Console.WriteLine(string.Concat(" ThirdEye XP: ", avatarDetail.Chakras.ThirdEye.XP));
        //    Console.WriteLine(string.Concat(" ThirdEye Level: ", avatarDetail.Chakras.ThirdEye.Level));
        //    Console.WriteLine(string.Concat(" Throat XP: ", avatarDetail.Chakras.Throat.XP));
        //    Console.WriteLine(string.Concat(" Throat Level: ", avatarDetail.Chakras.Throat.Level));
        //    Console.WriteLine(string.Concat(" Heart XP: ", avatarDetail.Chakras.Heart.XP));
        //    Console.WriteLine(string.Concat(" Heart Level: ", avatarDetail.Chakras.Heart.Level));
        //    Console.WriteLine(string.Concat(" SoloarPlexus XP: ", avatarDetail.Chakras.SoloarPlexus.XP));
        //    Console.WriteLine(string.Concat(" SoloarPlexus Level: ", avatarDetail.Chakras.SoloarPlexus.Level));
        //    Console.WriteLine(string.Concat(" Sacral XP: ", avatarDetail.Chakras.Sacral.XP));
        //    Console.WriteLine(string.Concat(" Sacral Level: ", avatarDetail.Chakras.Sacral.Level));

        //    Console.WriteLine(string.Concat(" Root SanskritName: ", avatarDetail.Chakras.Root.SanskritName));
        //    Console.WriteLine(string.Concat(" Root XP: ", avatarDetail.Chakras.Root.XP));
        //    Console.WriteLine(string.Concat(" Root Level: ", avatarDetail.Chakras.Root.Level));
        //    Console.WriteLine(string.Concat(" Root Progress: ", avatarDetail.Chakras.Root.Progress));
        //    // Console.WriteLine(string.Concat(" Root Color: ", Superavatar.Chakras.Root.Color.Name));
        //    Console.WriteLine(string.Concat(" Root Element: ", avatarDetail.Chakras.Root.Element.Name));
        //    Console.WriteLine(string.Concat(" Root YogaPose: ", avatarDetail.Chakras.Root.YogaPose.Name));
        //    Console.WriteLine(string.Concat(" Root WhatItControls: ", avatarDetail.Chakras.Root.WhatItControls));
        //    Console.WriteLine(string.Concat(" Root WhenItDevelops: ", avatarDetail.Chakras.Root.WhenItDevelops));
        //    Console.WriteLine(string.Concat(" Root Crystal Name: ", avatarDetail.Chakras.Root.Crystal.Name.Name));
        //    Console.WriteLine(string.Concat(" Root Crystal AmplifyicationLevel: ", avatarDetail.Chakras.Root.Crystal.AmplifyicationLevel));
        //    Console.WriteLine(string.Concat(" Root Crystal CleansingLevel: ", avatarDetail.Chakras.Root.Crystal.CleansingLevel));
        //    Console.WriteLine(string.Concat(" Root Crystal EnergisingLevel: ", avatarDetail.Chakras.Root.Crystal.EnergisingLevel));
        //    Console.WriteLine(string.Concat(" Root Crystal GroundingLevel: ", avatarDetail.Chakras.Root.Crystal.GroundingLevel));
        //    Console.WriteLine(string.Concat(" Root Crystal ProtectionLevel: ", avatarDetail.Chakras.Root.Crystal.ProtectionLevel));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Aurua:");
        //    Console.WriteLine(string.Concat(" Brightness: ", avatarDetail.Aura.Brightness));
        //    Console.WriteLine(string.Concat(" Size: ", avatarDetail.Aura.Size));
        //    Console.WriteLine(string.Concat(" Level: ", avatarDetail.Aura.Level));
        //    Console.WriteLine(string.Concat(" Value: ", avatarDetail.Aura.Value));
        //    Console.WriteLine(string.Concat(" Progress: ", avatarDetail.Aura.Progress));
        //    Console.WriteLine(string.Concat(" ColourRed: ", avatarDetail.Aura.ColourRed));
        //    Console.WriteLine(string.Concat(" ColourGreen: ", avatarDetail.Aura.ColourGreen));
        //    Console.WriteLine(string.Concat(" ColourBlue: ", avatarDetail.Aura.ColourBlue));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Attributes:");
        //    Console.WriteLine(string.Concat(" Strength: ", avatarDetail.Attributes.Strength));
        //    Console.WriteLine(string.Concat(" Speed: ", avatarDetail.Attributes.Speed));
        //    Console.WriteLine(string.Concat(" Dexterity: ", avatarDetail.Attributes.Dexterity));
        //    Console.WriteLine(string.Concat(" Intelligence: ", avatarDetail.Attributes.Intelligence));
        //    Console.WriteLine(string.Concat(" Magic: ", avatarDetail.Attributes.Magic));
        //    Console.WriteLine(string.Concat(" Wisdom: ", avatarDetail.Attributes.Wisdom));
        //    Console.WriteLine(string.Concat(" Toughness: ", avatarDetail.Attributes.Toughness));
        //    Console.WriteLine(string.Concat(" Vitality: ", avatarDetail.Attributes.Vitality));
        //    Console.WriteLine(string.Concat(" Endurance: ", avatarDetail.Attributes.Endurance));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Stats:");
        //    Console.WriteLine(string.Concat(" HP: ", avatarDetail.Stats.HP.Current, "/", avatarDetail.Stats.HP.Max));
        //    Console.WriteLine(string.Concat(" Mana: ", avatarDetail.Stats.Mana.Current, "/", avatarDetail.Stats.Mana.Max));
        //    Console.WriteLine(string.Concat(" Energy: ", avatarDetail.Stats.Energy.Current, "/", avatarDetail.Stats.Energy.Max));
        //    Console.WriteLine(string.Concat(" Staminia: ", avatarDetail.Stats.Staminia.Current, "/", avatarDetail.Stats.Staminia.Max));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Super Powers:");
        //    Console.WriteLine(string.Concat(" Flight: ", avatarDetail.SuperPowers.Flight));
        //    Console.WriteLine(string.Concat(" Astral Projection: ", avatarDetail.SuperPowers.AstralProjection));
        //    Console.WriteLine(string.Concat(" Bio-Locatation: ", avatarDetail.SuperPowers.BioLocatation));
        //    Console.WriteLine(string.Concat(" Heat Vision: ", avatarDetail.SuperPowers.HeatVision));
        //    Console.WriteLine(string.Concat(" Invulerability: ", avatarDetail.SuperPowers.Invulerability));
        //    Console.WriteLine(string.Concat(" Remote Viewing: ", avatarDetail.SuperPowers.RemoteViewing));
        //    Console.WriteLine(string.Concat(" Super Speed: ", avatarDetail.SuperPowers.SuperSpeed));
        //    Console.WriteLine(string.Concat(" Super Strength: ", avatarDetail.SuperPowers.SuperStrength));
        //    Console.WriteLine(string.Concat(" Telekineseis: ", avatarDetail.SuperPowers.Telekineseis));
        //    Console.WriteLine(string.Concat(" XRay Vision: ", avatarDetail.SuperPowers.XRayVision));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Skills:");
        //    Console.WriteLine(string.Concat(" Computers: ", avatarDetail.Skills.Computers));
        //    Console.WriteLine(string.Concat(" Engineering: ", avatarDetail.Skills.Engineering));
        //    Console.WriteLine(string.Concat(" Farming: ", avatarDetail.Skills.Farming));
        //    Console.WriteLine(string.Concat(" FireStarting: ", avatarDetail.Skills.FireStarting));
        //    Console.WriteLine(string.Concat(" Fishing: ", avatarDetail.Skills.Fishing));
        //    Console.WriteLine(string.Concat(" Languages: ", avatarDetail.Skills.Languages));
        //    Console.WriteLine(string.Concat(" Meditation: ", avatarDetail.Skills.Meditation));
        //    Console.WriteLine(string.Concat(" MelleeCombat: ", avatarDetail.Skills.MelleeCombat));
        //    Console.WriteLine(string.Concat(" Mindfulness: ", avatarDetail.Skills.Mindfulness));
        //    Console.WriteLine(string.Concat(" Negotiating: ", avatarDetail.Skills.Negotiating));
        //    Console.WriteLine(string.Concat(" RangeCombat: ", avatarDetail.Skills.RangeCombat));
        //    Console.WriteLine(string.Concat(" Research: ", avatarDetail.Skills.Research));
        //    Console.WriteLine(string.Concat(" Science: ", avatarDetail.Skills.Science));
        //    Console.WriteLine(string.Concat(" SpellCasting: ", avatarDetail.Skills.SpellCasting));
        //    Console.WriteLine(string.Concat(" Translating: ", avatarDetail.Skills.Translating));
        //    Console.WriteLine(string.Concat(" Yoga: ", avatarDetail.Skills.Yoga));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Gifts:");

        //    foreach (AvatarGift gift in avatarDetail.Gifts)
        //        Console.WriteLine(string.Concat(" ", Enum.GetName(gift.GiftType), " earnt on ", gift.GiftEarnt.ToString()));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Spells:");

        //    foreach (Spell spell in avatarDetail.Spells)
        //        Console.WriteLine(string.Concat(" ", spell.Name));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Inventory:");

        //    foreach (InventoryItem inventoryItem in avatarDetail.Inventory)
        //        Console.WriteLine(string.Concat(" ", inventoryItem.Name));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Achievements:");

        //    foreach (Achievement achievement in avatarDetail.Achievements)
        //        Console.WriteLine(string.Concat(" ", achievement.Name));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Gene Keys:");

        //    foreach (GeneKey geneKey in avatarDetail.GeneKeys)
        //        Console.WriteLine(string.Concat(" ", geneKey.Name));

        //    Console.WriteLine("");
        //    Console.WriteLine(" Human Design:");
        //    Console.WriteLine(string.Concat(" Type: ", avatarDetail.HumanDesign.Type));
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //}

        private static void STAR_OnInitialized(object sender, System.EventArgs e)
        {
            CLIEngine.ShowSuccessMessage(" STAR Initialized.");
        }

        private static void STAR_OnOASISBootError(object sender, OASISBootErrorEventArgs e)
        {
            //CLIEngine.ShowErrorMessage(string.Concat("OASIS Boot Error. Reason: ", e.ErrorReason));
            CLIEngine.ShowErrorMessage(e.ErrorReason);
        }

        private static void STAR_OnOASISBooted(object sender, EventArgs.OASISBootedEventArgs e)
        {
            // CLIEngine.ShowSuccessMessage(string.Concat("OASIS BOOTED.", e.Message));
        }

        private static void STAR_OnStarError(object sender, EventArgs.StarErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage(string.Concat("Error Igniting SuperStar. Reason: ", e.Reason));
        }

        private static void STAR_OnStarIgnited(object sender, System.EventArgs e)
        {
            //CLIEngine.ShowSuccessMessage("STAR IGNITED");
        }

        private static void STAR_OnStarStatusChanged(object sender, EventArgs.StarStatusChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                switch (e.MessageType)
                {
                    case Enums.StarStatusMessageType.Processing:
                        CLIEngine.ShowWorkingMessage(e.Message);
                        break;

                    case Enums.StarStatusMessageType.Success:
                        CLIEngine.ShowSuccessMessage(e.Message);
                        break;

                    case Enums.StarStatusMessageType.Error:
                        CLIEngine.ShowErrorMessage(e.Message);
                        break;
                }
            }
            else
            {
                switch (e.Status)
                {
                    case Enums.StarStatus.BootingOASIS:
                    //CLIEngine.ShowWorkingMessage("BOOTING OASIS...");
                    //break;

                    case Enums.StarStatus.OASISBooted:
                        //CLIEngine.ShowSuccessMessage("OASIS BOOTED"); //OASISBootLoader already shows this message so no need to show again! ;-)
                        break;

                    case Enums.StarStatus.Igniting:
                        CLIEngine.ShowWorkingMessage("IGNITING STAR...");
                        break;

                    case Enums.StarStatus.Ignited:
                        CLIEngine.ShowSuccessMessage("STAR IGNITED");
                        break;

                        //case Enums.SuperStarStatus.Error:
                        //  CLIEngine.ShowErrorMessage("SuperStar Error");
                }
            }
        }

        private static void STAR_OnCelestialSpacesLoaded(object sender, CelestialSpacesLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalSpaces Loaded Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialSpacesSaved(object sender, CelestialSpacesSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalSpaces Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialSpacesError(object sender, CelestialSpacesErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving CelestialSpaces. Reason: {e.Reason}");
        }

        private static void STAR_OnCelestialSpaceLoaded(object sender, CelestialSpaceLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalSpace Loaded Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialSpaceSaved(object sender, CelestialSpaceSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalSpace Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialSpaceError(object sender, CelestialSpaceErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving CelestialSpace. Reason: {e.Reason}");
        }

        private static void STAR_OnCelestialBodyLoaded(object sender, CelestialBodyLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalBody Loaded Successfully. {detailedMessage}");
        }
        private static void STAR_OnCelestialBodySaved(object sender, CelestialBodySavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            //CLIEngine.ShowSuccessMessage($"CelesitalBody Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialBodyError(object sender, CelestialBodyErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving CelestialBody. Reason: {e.Reason}");
        }

        private static void STAR_OnCelestialBodiesLoaded(object sender, CelestialBodiesLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalBodies Loaded Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialBodiesSaved(object sender, CelestialBodiesSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"CelesitalBodies Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnCelestialBodiesError(object sender, CelestialBodiesErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving CelestialBodies. Reason: {e.Reason}");
        }

        private static void STAR_OnZomeLoaded(object sender, ZomeLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"Zome Loaded Successfully. {detailedMessage}");
        }

        private static void STAR_OnZomeSaved(object sender, ZomeSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"Zome Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving Zome. Reason: {e.Reason}");
            //Console.WriteLine(string.Concat("Star Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails, "HoloNETErrorDetails.Reason: ", e.HoloNETErrorDetails.Reason, "HoloNETErrorDetails.ErrorDetails: ", e.HoloNETErrorDetails.ErrorDetails));
            //CLIEngine.ShowErrorMessage(string.Concat(" STAR Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails));
        }

        private static void STAR_OnZomesLoaded(object sender, ZomesLoadedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"Zome Loaded Successfully. {detailedMessage}");
        }

        private static void STAR_OnZomesSaved(object sender, ZomesSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"Zome Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnZomesError(object sender, ZomesErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving Zomes. Reason: {e.Reason}");
        }

        private static void STAR_OnHolonLoaded(object sender, HolonLoadedEventArgs e)
        {
            CLIEngine.ShowSuccessMessage(string.Concat(" STAR Holons Loaded. Holon Name: ", e.Result.Result.Name));
        }

        private static void STAR_OnHolonSaved(object sender, HolonSavedEventArgs e)
        {
            if (e.Result.IsError)
                CLIEngine.ShowErrorMessage(e.Result.Message);
            else
                CLIEngine.ShowSuccessMessage(string.Concat("STAR Holons Saved. Holon Saved: ", e.Result.Result.Name));
        }

        private static void STAR_OnHolonError(object sender, HolonErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving Holon. Reason: {e.Reason}");
        }

        private static void STAR_OnHolonsLoaded(object sender, HolonsLoadedEventArgs e)
        {
            CLIEngine.ShowSuccessMessage(string.Concat(" STAR Holons Loaded. Holons Loaded: ", e.Result.Result.Count()));
        }

        private static void STAR_OnHolonsSaved(object sender, HolonsSavedEventArgs e)
        {
            string detailedMessage = string.IsNullOrEmpty(e.Result.Message) ? e.Result.Message : "";
            CLIEngine.ShowSuccessMessage($"Holons Saved Successfully. {detailedMessage}");
        }

        private static void STAR_OnHolonsError(object sender, HolonsErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving Holons. Reason: {e.Reason}");
        }

        private static void StarCore_OnZomeError(object sender, ZomeErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"Error occured loading/saving Zome For StarCore. Reason: {e.Reason}");
            //Console.WriteLine(string.Concat("Star Core Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails, "HoloNETErrorDetails.Reason: ", e.HoloNETErrorDetails.Reason, "HoloNETErrorDetails.ErrorDetails: ", e.HoloNETErrorDetails.ErrorDetails));
            //CLIEngine.ShowErrorMessage(string.Concat(" Star Core Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails));
        }


        //TODO: Obsolete?
        //private static void OurWorld_OnZomeError(object sender, ZomeErrorEventArgs e)
        //{
        //    CLIEngine.ShowErrorMessage($"Error occured loading/saving Zome For Planet Our World. Reason: {e.Reason}");
        //    //Console.WriteLine(string.Concat("Our World Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails, "HoloNETErrorDetails.Reason: ", e.HoloNETErrorDetails.Reason, "HoloNETErrorDetails.ErrorDetails: ", e.HoloNETErrorDetails.ErrorDetails));
        //    //CLIEngine.ShowErrorMessage(string.Concat(" Our World Error Occured. EndPoint: ", e.EndPoint, ". Reason: ", e.Reason, ". Error Details: ", e.ErrorDetails));
        //}

        //TODO: Obsolete?
        //private static void OurWorld_OnHolonLoaded(object sender, HolonLoadedEventArgs e)
        //{
        //    Console.WriteLine(" Holon Loaded");
        //    Console.WriteLine(string.Concat(" Holon Id: ", e.Result.Result.Id));
        //    Console.WriteLine(string.Concat(" Holon ProviderUniqueStorageKey: ", e.Result.Result.ProviderUniqueStorageKey));
        //    Console.WriteLine(string.Concat(" Holon Name: ", e.Result.Result.Name));
        //    Console.WriteLine(string.Concat(" Holon Type: ", e.Result.Result.HolonType));
        //    Console.WriteLine(string.Concat(" Holon Description: ", e.Result.Result.Description));

        //    //Console.WriteLine(string.Concat("ourWorld.Zomes[0].Holons[0].ProviderUniqueStorageKey: ", ourWorld.Zomes[0].Holons[0].ProviderUniqueStorageKey));
        //    Console.WriteLine(string.Concat(" ourWorld.Zomes[0].Holons[0].ProviderUniqueStorageKey: ", _superWorld.CelestialBodyCore.Zomes[0].Children[0].ProviderUniqueStorageKey));
        //}

        //TODO: Obsolete?
        //private static void OurWorld_OnHolonSaved(object sender, HolonSavedEventArgs e)
        //{
        //    if (e.Result.IsError)
        //        CLIEngine.ShowErrorMessage(e.Result.Message);
        //    else
        //    {
        //        Console.WriteLine(" Holon Saved");
        //        Console.WriteLine(string.Concat(" Holon Id: ", e.Result.Result.Id));
        //        Console.WriteLine(string.Concat(" Holon ProviderUniqueStorageKey: ", e.Result.Result.ProviderUniqueStorageKey));
        //        Console.WriteLine(string.Concat(" Holon Name: ", e.Result.Result.Name));
        //        Console.WriteLine(string.Concat("Holon Type: ", e.Result.Result.HolonType));
        //        Console.WriteLine(string.Concat(" Holon Description: ", e.Result.Result.Description));

        //        Console.WriteLine(" Loading Holon...");
        //        //ourWorld.CelestialBodyCore.LoadHolonAsync(e.Holon.Name, e.Holon.ProviderUniqueStorageKey);
        //        _superWorld.CelestialBodyCore.GlobalHolonData.LoadHolonAsync(e.Result.Result.Id);
        //    }
        //}
    }
}
