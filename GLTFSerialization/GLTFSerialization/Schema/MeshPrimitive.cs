using System.Collections.Generic;
using GLTF.Extensions;
using Newtonsoft.Json;

namespace GLTF.Schema
{
	/// <summary>
	/// Geometry to be rendered with the given material.
	/// </summary>
	public class MeshPrimitive : GLTFProperty
	{
		/// <summary>
		/// A dictionary object, where each key corresponds to mesh attribute semantic
		/// and each value is the index of the accessor containing attribute's data.
		/// </summary>
		public Dictionary<string, AccessorId> Attributes = new Dictionary<string, AccessorId>();

		/// <summary>
		/// The index of the accessor that contains mesh indices.
		/// When this is not defined, the primitives should be rendered without indices
		/// using `drawArrays()`. When defined, the accessor must contain indices:
		/// the `bufferView` referenced by the accessor must have a `target` equal
		/// to 34963 (ELEMENT_ARRAY_BUFFER); a `byteStride` that is tightly packed,
		/// i.e., 0 or the byte size of `componentType` in bytes;
		/// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT)
		/// or 5125 (UNSIGNED_INT), the latter is only allowed
		/// when `OES_element_index_uint` extension is used; `type` must be `\"SCALAR\"`.
		/// </summary>
		public AccessorId Indices;

		/// <summary>
		/// The index of the material to apply to this primitive when rendering.
		/// </summary>
		public MaterialId Material;

		/// <summary>
		/// The type of primitives to render. All valid values correspond to WebGL enums.
		/// </summary>
		public DrawMode Mode = DrawMode.Triangles;

		/// <summary>
		/// An array of Morph Targets, each  Morph Target is a dictionary mapping
		/// attributes (only "POSITION" and "NORMAL" supported) to their deviations
		/// in the Morph Target (index of the accessor containing the attribute
		/// displacements' data).
		/// </summary>
		/// TODO: Make dictionary key enums?
		public List<Dictionary<string, AccessorId>> Targets;

		public MeshPrimitive()
		{
		}

		public MeshPrimitive(MeshPrimitive meshPrimitive, GLTFRoot gltfRoot) : base(meshPrimitive)
		{
			if (meshPrimitive == null) return;

			if (meshPrimitive.Attributes != null)
			{
				Attributes = new Dictionary<string, AccessorId>(meshPrimitive.Attributes.Count);
				foreach (KeyValuePair<string, AccessorId> attributeKeyValuePair in meshPrimitive.Attributes)
				{
					Attributes[attributeKeyValuePair.Key] = new AccessorId(attributeKeyValuePair.Value, gltfRoot);
				}
			}
			
			if (meshPrimitive.Indices != null)
			{
				Indices = new AccessorId(meshPrimitive.Indices, gltfRoot);
			}

			if (meshPrimitive.Material != null)
			{
				Material = new MaterialId(meshPrimitive.Material, gltfRoot);
			}

			Mode = meshPrimitive.Mode;

			if (meshPrimitive.Targets != null)
			{
				Targets = new List<Dictionary<string, AccessorId>>(meshPrimitive.Targets.Count);
				foreach (Dictionary<string, AccessorId> targetToCopy in meshPrimitive.Targets)
				{
					Dictionary<string, AccessorId> target = new Dictionary<string, AccessorId>(targetToCopy.Count);
					foreach (KeyValuePair<string, AccessorId> targetKeyValuePair in targetToCopy)
					{
						target[targetKeyValuePair.Key] = new AccessorId(targetKeyValuePair.Value, gltfRoot);
					}
					Targets.Add(target);
				}
			}
		}

		public static MeshPrimitive Deserialize(GLTFRoot root, JsonReader reader)
		{
			var primitive = new MeshPrimitive();

			while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
			{
				var curProp = reader.Value.ToString();

				switch (curProp)
				{
					case "attributes":
						primitive.Attributes = reader.ReadAsDictionary(() => new AccessorId
						{
							Id = reader.ReadAsInt32().Value,
							Root = root
						});
						break;
					case "indices":
						primitive.Indices = AccessorId.Deserialize(root, reader);
						break;
					case "material":
						primitive.Material = MaterialId.Deserialize(root, reader);
						break;
					case "mode":
						primitive.Mode = (DrawMode)reader.ReadAsInt32().Value;
						break;
					case "targets":
						primitive.Targets = reader.ReadList(() =>
						{
							return reader.ReadAsDictionary(() => new AccessorId
							{
								Id = reader.ReadAsInt32().Value,
								Root = root
							});
						});
						break;
					default:
						primitive.DefaultPropertyDeserializer(root, reader);
						break;
				}
			}

			return primitive;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();

			writer.WritePropertyName("attributes");
			writer.WriteStartObject();
			foreach (var attribute in Attributes)
			{
				writer.WritePropertyName(attribute.Key);
				writer.WriteValue(attribute.Value.Id);
			}
			writer.WriteEndObject();

			if (Indices != null)
			{
				writer.WritePropertyName("indices");
				writer.WriteValue(Indices.Id);
			}

			if (Material != null)
			{
				writer.WritePropertyName("material");
				writer.WriteValue(Material.Id);
			}

			if (Mode != DrawMode.Triangles)
			{
				writer.WritePropertyName("mode");
				writer.WriteValue((int)Mode);
			}

			if (Targets != null && Targets.Count > 0)
			{
				writer.WritePropertyName("targets");
				writer.WriteStartArray();
				foreach (var target in Targets)
				{
					writer.WriteStartObject();

					foreach (var attribute in target)
					{
						writer.WritePropertyName(attribute.Key);
						writer.WriteValue(attribute.Value.Id);
					}

					writer.WriteEndObject();
				}
				writer.WriteEndArray();
			}

			base.Serialize(writer);

			writer.WriteEndObject();
		}
	}

	public static class SemanticProperties
	{
		public static readonly string POSITION = "POSITION";
		public static readonly string NORMAL = "NORMAL";
		public static readonly string JOINT = "JOINT";
		public static readonly string WEIGHT = "WEIGHT";
		public static readonly string TANGENT = "TANGENT";
		public static readonly string INDICES = "INDICIES";

		/// <summary>
		/// Return the semantic property for the uv buffer.
		/// </summary>
		/// <param name="index">The index of the uv buffer</param>
		/// <returns>The semantic property for the uv buffer</returns>
		public static string TexCoord(int index)
		{
			return "TEXCOORD_" + index;
		}

		/// <summary>
		/// Return the semantic property for the color buffer.
		/// </summary>
		/// <param name="index">The index of the color buffer</param>
		/// <returns>The semantic property for the color buffer</returns>
		public static string Color(int index)
		{
			return "COLOR_" + index;
		}

		/// <summary>
		/// Return the semantic property for the bone weights buffer.
		/// </summary>
		/// <param name="index">The index of the bone weights buffer</param>
		/// <returns>The semantic property for the bone weights buffer</returns>
		public static string Weight(int index)
		{
			return "WEIGHTS_" + index;
		}

		/// <summary>
		/// Return the semantic property for the joints buffer.
		/// </summary>
		/// <param name="index">The index of the joints buffer</param>
		/// <returns>The semantic property for the joints buffer</returns>
		public static string Joint(int index)
		{
			return "JOINTS_" + index;
		}

		/// <summary>
		/// Parse out the index of a given semantic property.
		/// </summary>
		/// <param name="property">Semantic property to parse</param>
		/// <param name="index">Parsed index to assign</param>
		/// <returns></returns>
		public static bool ParsePropertyIndex(string property, out int index)
		{
			index = -1;
			var parts = property.Split('_');

			if (parts.Length != 2)
			{
				return false;
			}

			if (!int.TryParse(parts[1], out index))
			{
				return false;
			}

			return true;
		}
	}

	public enum DrawMode
	{
		Points = 0,
		Lines = 1,
		LineLoop = 2,
		LineStrip = 3,
		Triangles = 4,
		TriangleStrip = 5,
		TriangleFan = 6
	}
}
