﻿//====================================================================================================================
// Copyright (c) 2012 IdeaBlade
//====================================================================================================================
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS 
// OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
//====================================================================================================================
// USE OF THIS SOFTWARE IS GOVERENED BY THE LICENSING TERMS WHICH CAN BE FOUND AT
// http://cocktail.ideablade.com/licensing
//====================================================================================================================

using System;
using System.Collections.Generic;
using Cocktail;
using IdeaBlade.Core;
using IdeaBlade.EntityModel;

namespace Common.Repositories
{
    public abstract class RepositoryBase<T> : IRepository
        where T : EntityManager
    {
        private readonly RepositoryBase<T> _baseData;
        private readonly IEntityManagerProvider<T> _entityManagerProvider;

        protected RepositoryBase(IEntityManagerProvider<T> entityManagerProvider, RepositoryBase<T> baseData = null)
        {
            _entityManagerProvider = entityManagerProvider;
            _baseData = baseData;
            _entityManagerProvider.ManagerCreated += new EventHandler<EntityManagerCreatedEventArgs>(OnManagerCreated)
                .MakeWeak(eh => _entityManagerProvider.ManagerCreated -= eh);
        }

        protected T Manager
        {
            get { return _entityManagerProvider.Manager; }
        }

        public QueryStrategy BaseDataQueryStrategy
        {
            get { return _baseData != null ? QueryStrategy.CacheOnly : Manager.DefaultQueryStrategy; }
        }

        #region IRepository Members

        public bool HasChanges()
        {
            return Manager.HasChanges();
        }

        public OperationResult<SaveResult> SaveAsync(Action onSuccess = null, Action<Exception> onFail = null)
        {
            return Manager.SaveChangesAsync(op => op.OnComplete(onSuccess, onFail)).AsOperationResult();
        }

        public void RejectChanges()
        {
            Manager.RejectChanges();
        }

        #endregion

        protected internal virtual void OnManagerCreated(object sender, EntityManagerCreatedEventArgs e)
        {
            if (_baseData == null) return;

            var restoreStrategy = new RestoreStrategy(false, false, MergeStrategy.OverwriteChanges);
            EntityCacheState baseData = _baseData.Manager.CacheStateManager.GetCacheState();
            e.EntityManager.CacheStateManager.RestoreCacheState(baseData, restoreStrategy);
        }

        protected OperationResult<IEnumerable<TQuery>> ExecuteQuery<TQuery>(IEntityQuery<TQuery> query,
                                                                            Action<IEnumerable<TQuery>> onSuccess,
                                                                            Action<Exception> onFail)
        {
            return query.ExecuteAsync(op => op.OnComplete(onSuccess, onFail)).AsOperationResult();
        }
    }
}