﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Knight.Framework.Hotfix;
using UnityEngine;
using System.IO;

namespace Knight.Framework.TypeResolve
{
    public abstract class TypeResolveAssembly
    {
        public string       AssemblyName;
        public bool         IsHotfix;

        public TypeResolveAssembly(string rAssemblyName)
        {
            this.AssemblyName = rAssemblyName;
            this.IsHotfix     = false;
        }

        public virtual void Load()
        {
        }

        public virtual Type[] GetAllTypes()
        {
            return null;
        }

        public virtual object Instantiate(string rTypeName, params object[] rArgs)
        {
            return null;
        }

        public virtual T Instantiate<T>(string rTypeName, params object[] rArgs)
        {
            return default(T);
        }
    }

    public class TypeResolveAssembly_Mono : TypeResolveAssembly
    {
        private Assembly    mAssembly;

        public TypeResolveAssembly_Mono(string rAssemblyName) 
            : base(rAssemblyName)
        {
            this.IsHotfix = false;
        }

        public override void Load()
        {
            var rAllAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < rAllAssemblies.Length; i++)
            {
                if (rAllAssemblies[i].GetName().Name.Equals(this.AssemblyName))
                {
                    this.mAssembly = rAllAssemblies[i];
                    break;
                }
            }
        }

        public override Type[] GetAllTypes()
        {
            if (this.mAssembly == null) return new Type[0];
            return this.mAssembly.GetTypes();
        }

        public override object Instantiate(string rTypeName, params object[] rArgs)
        {
            if (this.mAssembly == null) return null;
            return Activator.CreateInstance(this.mAssembly.GetType(rTypeName), rArgs);
        }

        public override T Instantiate<T>(string rTypeName, params object[] rArgs)
        {
            if (this.mAssembly == null) return default(T);
            return (T)Activator.CreateInstance(this.mAssembly.GetType(rTypeName), rArgs);
        }
    }

    public class TypeResolveAssembly_Hotfix : TypeResolveAssembly
    {
        public TypeResolveAssembly_Hotfix(string rAssemblyName)
            : base(rAssemblyName)
        {
            this.IsHotfix = true;
        }

        public override void Load()
        {
#if UNITY_EDITOR
            // 编辑器下初始化
            if (!Application.isPlaying)
            {
                string rDLLPath = HotfixManager.HotfixDllDir + this.AssemblyName + ".bytes";
                string rPDBPath = HotfixManager.HotfixDllDir + this.AssemblyName + "_PDB.bytes";

                var rDLLBytes = File.ReadAllBytes(rDLLPath);
                var rPDBBytes = File.ReadAllBytes(rPDBPath);

                HotfixManager.Instance.Initialize();
                HotfixManager.Instance.InitApp(rDLLBytes, rPDBBytes); 
            }
#endif
        }

        public override Type[] GetAllTypes()
        {
            return HotfixManager.Instance.GetTypes();
        }

        public override object Instantiate(string rTypeName, params object[] rArgs)
        {
            return HotfixManager.Instance.Instantiate(rTypeName, rArgs);
        }

        public override T Instantiate<T>(string rTypeName, params object[] rArgs)
        {
            return HotfixManager.Instance.Instantiate<T>(rTypeName, rArgs);
        }
    }
}
