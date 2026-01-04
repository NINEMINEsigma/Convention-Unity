using System;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("dynamicFriction", "staticFriction", "bounciness", "frictionCombine", "bounceCombine")]
	public class ES3Type_PhysicMaterial : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3Type_PhysicMaterial() : base(typeof(
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterial
#else
							UnityEngine.PhysicMaterial
#endif
			))
		{ Instance = this; }

		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterial
#else
							UnityEngine.PhysicMaterial
#endif
							)obj;

			writer.WriteProperty("dynamicFriction", instance.dynamicFriction, ES3Type_float.Instance);
			writer.WriteProperty("staticFriction", instance.staticFriction, ES3Type_float.Instance);
			writer.WriteProperty("bounciness", instance.bounciness, ES3Type_float.Instance);
			writer.WriteProperty("frictionCombine", instance.frictionCombine);
			writer.WriteProperty("bounceCombine", instance.bounceCombine);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterial
#else
							UnityEngine.PhysicMaterial
#endif
							)obj;
			foreach (string propertyName in reader.Properties)
			{
				switch (propertyName)
				{

					case "dynamicFriction":
						instance.dynamicFriction = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "staticFriction":
						instance.staticFriction = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "bounciness":
						instance.bounciness = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "frictionCombine":
						instance.frictionCombine = reader.Read<
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterialCombine
#else
							UnityEngine.PhysicMaterialCombine
#endif
>();
						break;
					case "bounceCombine":
						instance.bounceCombine = reader.Read<
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterialCombine
#else
							UnityEngine.PhysicMaterialCombine
#endif
>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterial
#else
							UnityEngine.PhysicMaterial
#endif
();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

	public class ES3Type_PhysicMaterialArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3Type_PhysicMaterialArray() : base(typeof(
#if UNITY_6000_0_OR_NEWER
							UnityEngine.PhysicsMaterial
#else
							UnityEngine.PhysicMaterial
#endif
[]), ES3Type_PhysicMaterial.Instance)
		{
			Instance = this;
		}
	}
}