using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickC.Types
{
    public partial class TypeResolver
    {
        private ResolveObject defaultResolveObject;
        private List<ResolveObject> resolveObjects = new List<ResolveObject>();
        internal ResolveTypeWrite DefaultWriterResolve => defaultResolveObject.resolveTypeWrite;
        internal ResolveTypeRead DefaultReaderResolve => defaultResolveObject.resolveTypeRead;

        public void RegisterResolve(ResolveTypeWrite writeResolve, ResolveTypeRead readResolve, params Type[] allowTypes)
        {
            resolveObjects.Add(new ResolveObject
            {
                allowTypes = allowTypes,
                resolveTypeWrite = writeResolve,
                resolveTypeRead = readResolve,
            });
        }
        internal ResolveTypeWrite? FindWriterResolve(Type type)
        {
            var rObj = resolveObjects.FirstOrDefault(x => x.TypeContains(type));
            if (rObj == null)
                return null;
            return rObj.resolveTypeWrite;
        }
        internal ResolveTypeRead? FindReaderResolve(Type type)
        {
            var rObj = resolveObjects.FirstOrDefault(x => x.TypeContains(type));
            if (rObj == null)
                return null;
            return rObj.resolveTypeRead;
        }
    }
}
