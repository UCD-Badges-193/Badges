﻿using System;
using Badges.Core.Domain;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;

namespace Badges.Core.Repositories
{
    public interface IRepositoryFactory
    {
        IRepositoryWithTypedId<User, Guid> UserRepository { get; set; }
        IRepositoryWithTypedId<Profile, Guid> ProfileRepository { get; set; }
        void Flush();
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepositoryWithTypedId<User, Guid> UserRepository { get; set; }
        public IRepositoryWithTypedId<Profile, Guid> ProfileRepository { get; set; }

        public void Flush()
        {
            NHibernateSessionManager.Instance.GetSession().Flush();
        }
    }
}
