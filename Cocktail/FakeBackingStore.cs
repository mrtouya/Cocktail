// ====================================================================================================================
//   Copyright (c) 2012 IdeaBlade
// ====================================================================================================================
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//   WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS 
//   OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//   OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// ====================================================================================================================
//   USE OF THIS SOFTWARE IS GOVERENED BY THE LICENSING TERMS WHICH CAN BE FOUND AT
//   http://cocktail.ideablade.com/licensing
// ====================================================================================================================

using System.Collections.Generic;
using IdeaBlade.Core.Composition;
using System.Threading.Tasks;
using IdeaBlade.EntityModel;

namespace Cocktail
{
    internal partial class FakeBackingStore
    {
        private static readonly Dictionary<string, FakeBackingStore> FakeBackingStores =
            new Dictionary<string, FakeBackingStore>();

        private readonly string _compositionContextName;
        private IEntityServerFakeBackingStore _store;

        public FakeBackingStore(string compositionContextName)
        {
            _compositionContextName = compositionContextName;
        }

        private CompositionContext CompositionContext
        {
            get { return CompositionContext.GetByName(_compositionContextName); }
        }

        private IEntityServerFakeBackingStore Store
        {
            get { return _store ?? (_store = CompositionContext.GetFakeBackingStore()); }
        }

        public bool IsInitialized { get; private set; }

        public async Task ResetAsync(EntityCacheState storeEcs)
        {

#if !SILVERLIGHT && !NETFX_CORE
            // If the local fake backing store is used, do it synchronously
            if (Store is EntityServerFakeBackingStore.Local)
            {
                Reset(storeEcs);
                return;
            }
#endif
            // Clear all data from the backing store
            await Store.ClearAsync();
            await Store.RestoreAsync(storeEcs);

            IsInitialized = true;
        }

        public static bool Exists(string compositionContextName)
        {
            return FakeBackingStores.ContainsKey(compositionContextName);
        }

        public static FakeBackingStore Get(string compositionContextName)
        {
            return FakeBackingStores[compositionContextName];
        }

        public static FakeBackingStore Create(string compositionContextName)
        {
            var fakeBackingStore = new FakeBackingStore(compositionContextName);
            FakeBackingStores.Add(compositionContextName, fakeBackingStore);

            return fakeBackingStore;
        }
    }
}