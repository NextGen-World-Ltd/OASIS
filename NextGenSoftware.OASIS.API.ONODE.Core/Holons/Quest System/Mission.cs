﻿using System;
using System.Collections.Generic;
using NextGenSoftware.OASIS.API.Core.Enums;
using NextGenSoftware.OASIS.API.Core.CustomAttrbiutes;
using NextGenSoftware.OASIS.API.ONODE.Core.Interfaces;
using NextGenSoftware.OASIS.API.ONODE.Core.Interfaces.Holons;

namespace NextGenSoftware.OASIS.API.ONODE.Core.Holons
{
    //public class Mission : TaskBase, IMission, ITaskBase, IPublishableHolon //TODO: SOOOOOO wish we could extend both PublishableHolon and TaskBase! :(
    //public class Mission : PublishableHolon, IMission, ITaskBase, IPublishableHolon
    public class Mission : STARHolon, IMission, ITaskBase, ISTARHolon
    {
        //private IAvatar _publishedByAvatar = null;

        public Mission()
        {
            this.HolonType = HolonType.Mission; 
        }

        [CustomOASISProperty()]
        public DateTime StartedOn { get; set; }

        [CustomOASISProperty()]
        public Guid StartedBy { get; set; }

        [CustomOASISProperty()]
        public DateTime CompletedOn { get; set; }

        [CustomOASISProperty()]
        public Guid CompletedBy { get; set; }

        //[CustomOASISProperty]
        //public DateTime PublishedOn { get; set; }

        //[CustomOASISProperty]
        //public Guid PublishedByAvatarId { get; set; }

        //public IAvatar PublishedByAvatar
        //{
        //    get
        //    {
        //        if (_publishedByAvatar == null && PublishedByAvatarId != Guid.Empty)
        //        {
        //            OASISResult<IAvatar> avatarResult = AvatarManager.Instance.LoadAvatar(PublishedByAvatarId);

        //            if (avatarResult != null && avatarResult.Result != null && !avatarResult.IsError)
        //                _publishedByAvatar = avatarResult.Result;
        //        }

        //        return _publishedByAvatar;
        //    }
        //}

        [CustomOASISProperty]
        public IList<IQuest> Quests { get; set; } = new List<IQuest>();

        [CustomOASISProperty]
        public IList<IChapter> Chapters { get; set; } = new List<IChapter>(); //optional (large collection of quests can be broken into chapters.)
    }
}