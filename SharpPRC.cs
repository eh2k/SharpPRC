/************
*
*   Copyright (C) 2014  eheidt <https://github.com/eh2k>
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU Lesser General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU Lesser General Public License for more details.
*
*   You should have received a copy of the GNU Lesser General Public License
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*************/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace SharpPRC
{
    //http://help.adobe.com/livedocs/acrobat_sdk/9/Acrobat9_HTMLHelp/API_References/PRCReference/PRC_Format_Specification/index.html

    public class PRCbitStreamWriter
    {
        private readonly StreamWriter _streamWriter;

        PRCbitStreamWriter(Stream stream)
        {
            _streamWriter = new StreamWriter(
                new DeflateStream(stream, CompressionLevel.Optimal));
        }

        public void WriteString(string value)
        {
            _streamWriter.Write(value);
        }

        public void WriteBool(bool value)
        {
            _streamWriter.Write(value);
        }

        public void WriteDouble(double value)
        {
            _streamWriter.Write(value);
        }

        public void WriteUint32(UInt32 value)
        {
            _streamWriter.Write(value);
        }

        public void WriteUInt32List(List<UInt32> list)
        {
            UInt32 size = (UInt32)list.Count;
            _streamWriter.Write(size);
            foreach (var value in list)
                _streamWriter.Write(value);
        }

        public void WriteDoubleList(List<double> list)
        {
            UInt32 size = (UInt32)list.Count;
            _streamWriter.Write(size);
            foreach (var value in list)
                _streamWriter.Write(value);
        }

        internal void WriteUncompressedBlock(string p1, int p2)
        {
            throw new NotImplementedException();
        }

        internal void WriteUncompressedUnsignedInteger(uint minimal_version_for_read)
        {
            throw new NotImplementedException();
        }

        internal void SerializeFileStructureUncompressedUniqueId(PRCUniqueId file_structure_uuid)
        {
            throw new NotImplementedException();
        }

        private void SerializeContentBaseTessData(PRCContentBaseTessData t)
        {
            WriteBool(t.is_calculated);
            WriteDoubleList(t.coordinates);
        }

        private void SerializeContent3DTess(PRC3DTess t)
        {
            SerializeContentBaseTessData(t);

            WriteBool(t.has_faces);
            WriteBool(t.has_loops);
            WriteBool(false); //must recalculate normals
            WriteDoubleList(t.normal_coordinate);
            WriteUInt32List(t.wire_index);
            WriteUInt32List(t.triangulated_index);

            WriteUint32((UInt32)t.face_tessellation.Count);
            foreach (var face in t.face_tessellation)
                SerializeContentTessFace(face);

            WriteDoubleList(t.texture_coordinate);
        }

        public void Serialize3DTess(PRC3DTess t)
        {
            WriteUint32(PRCType.PRC_TYPE_TESS_3D);
            SerializeContent3DTess(t);
        }

        public void SerializeContentTessFace(PRCTessFace t)
        {
            WriteUint32(PRCType.PRC_TYPE_TESS_Face);
            WriteUInt32List(t.line_attributes);
            WriteUint32(t.start_wire);
            WriteUInt32List(t.sizes_wire);
            WriteUint32(t.used_entities_flag);
            WriteUint32(t.start_triangulated);
            WriteUInt32List(t.sizes_triangulated);

            WriteUint32(t.number_of_texture_coordinate_indexes);

            WriteBool(false); //HasVertexColors = False
            if (t.line_attributes.Count > 0)
                WriteUint32(t.behaviour);
        }

        protected void SerializeContentPRCBase(PRCContentBase t)
        {
            WriteUint32((UInt32)0); //Attributes
            WriteString(t.name);
            WriteUint32(t.CAD_identifier);
            WriteUint32(t.CAD_persistent_identifier);
            WriteUint32(t.PRC_unique_identifier);
        }

        public void SerializeContentMaterial(PRCMaterial t)
        {
            WriteUint32(PRCType.PRC_TYPE_GRAPH_Material);

            SerializeContentPRCBase(t);

            WriteUint32(t.ambient + 1);
            WriteUint32(t.diffuse + 1);
            WriteUint32(t.emissive + 1);
            WriteUint32(t.specular + 1);
            WriteDouble(t.shininess);
            WriteDouble(t.ambient_alpha);
            WriteDouble(t.diffuse_alpha);
            WriteDouble(t.emissive_alpha);
            WriteDouble(t.specular_alpha);
        }

        private void SerializeGraphics(PRCPolyBrepModel outStream)
        {
            WriteBool(false);
        }

        private void SerializePRCBaseWithGraphics(PRCPolyBrepModel t)
        {
            SerializeContentPRCBase(t);
            SerializeGraphics(t);
        }

        private void SerializeRepresentationItemContent(PRCPolyBrepModel t)
        {
            SerializePRCBaseWithGraphics(t);
            WriteUint32(t.index_local_coordinate_system + 1);
            WriteUint32(t.index_tessellation + 1);
        }

        private void SerializeUserData<T>(T t)
        {
            WriteUint32(0); //No UserData
        }

        public void SerializePolyBrepModel(PRCPolyBrepModel t)
        {
            WriteUint32(PRCType.PRC_TYPE_RI_PolyBrepModel);
            SerializeRepresentationItemContent(t);
            WriteBool(t.is_closed);
            SerializeUserData(t);
        }

        public void SerializeFileStructure(PRCFileStructure t)
        {
            t.h
        }

        public void SerializeFileStructureInformation(PRCFileStructure t)
        {
            throw new NotImplementedException();
        }

        public void SerializeStartHeader(PRCStartHeader t)
        {
            WriteUncompressedBlock("PRC", 3);
            WriteUncompressedUnsignedInteger(t.minimal_version_for_read);
            WriteUncompressedUnsignedInteger(t.authoring_version);
            SerializeFileStructureUncompressedUniqueId(t.file_structure_uuid);
            SerializeFileStructureUncompressedUniqueId(t.application_uuid);
        }

        public void SerializePRCHeader(PRCHeader t)
        {
            SerializeStartHeader(t);

            WriteUint32((UInt32)t.file_structure_information.Count);
            foreach (var fileStructure in t.file_structure_information)
                SerializeFileStructureInformation(fileStructure);

            WriteUint32(t.model_file_offset);
            WriteUint32(t.file_size);
            WriteUint32(0); //No UncompressedFiles
        }

        public void SerializeSheme(PRCFile f)
        {
            WriteUint32(0); //No Shema
        }

        public void SerializeUnit(PRCUnit u)
        {
            WriteBool(u.unit_from_CAD_file);
            WriteDouble(u.unit);
        }

        public void SerializeModelFileData(PRCFile f)
        {
            WriteUint32(PRCType.PRC_TYPE_ASM_ModelFile);
            SerializeContentPRCBase(f);
            SerializeUnit(f.unit);
            WriteUint32(0); //product_occurences
            WriteUint32(0); //file_structure_index_in_model_file
            SerializeUserData(f);
        }

        public void SerializePRC(PRCFile f)
        {         
            f._header.file_structure_information.Clear();
            f._header.file_structure_information.AddRange(f._fileStructures);
            f._header.file_size = f.GetSize();
            f._header.model_file_offset = f._header.file_size - 

            SerializePRCHeader(f._header);

            foreach (var fileStructure in f._fileStructures)
                SerializeFileStructure(fileStructure);

            SerializeSheme(f);
            SerializeModelFileData(f);
        }
    }

    static class PRCType
    {
        public const UInt32 PRC_TYPE_TESS = 170;
        public const UInt32 PRC_TYPE_TESS_Base = (PRC_TYPE_TESS + 1);
        public const UInt32 PRC_TYPE_TESS_3D = (PRC_TYPE_TESS + 2);
        public const UInt32 PRC_TYPE_TESS_3D_Compressed = (PRC_TYPE_TESS + 3);
        public const UInt32 PRC_TYPE_TESS_Face = (PRC_TYPE_TESS + 4);
        public const UInt32 PRC_TYPE_TESS_3D_Wire = (PRC_TYPE_TESS + 5);
        public const UInt32 PRC_TYPE_TESS_Markup = (PRC_TYPE_TESS + 6);

        public const UInt32 PRC_TYPE_RI = 230;

        public const UInt32 PRC_TYPE_RI_PolyBrepModel = (PRC_TYPE_RI + 7);

        public const UInt32 PRC_TYPE_ASM = 300;
        public const UInt32 PRC_TYPE_ASM_ModelFile = (PRC_TYPE_ASM + 1);

        public const UInt32 PRC_TYPE_GRAPH = (700);
        public const UInt32 PRC_TYPE_GRAPH_Material = (PRC_TYPE_GRAPH + 2);

    }

    public class PRCContentBaseTessData
    {
        internal bool is_calculated = false;
        internal readonly List<double> coordinates = new List<double>();
    }

    public class PRC3DTess : PRCContentBaseTessData
    {
        internal bool has_faces = false;
        internal bool has_loops = false;
        internal readonly List<double> normal_coordinate = new List<double>();
        internal readonly List<UInt32> wire_index = new List<UInt32>();
        internal readonly List<UInt32> triangulated_index = new List<UInt32>();
        internal readonly List<PRCTessFace> face_tessellation = new List<PRCTessFace>();
        internal readonly List<double> texture_coordinate = new List<double>();
    }

    public class PRCTessFace
    {
        internal const UInt32 PRC_GRAPHICS_Show = 0x0001;
        internal const UInt32 PRC_FACETESSDATA_Triangle = 0x0002;
        internal readonly List<UInt32> line_attributes = new List<UInt32>();
        internal UInt32 start_wire = 0;
        internal readonly List<UInt32> sizes_wire = new List<UInt32>();
        internal UInt32 used_entities_flag = 0;
        internal UInt32 start_triangulated = 0;
        internal readonly List<UInt32> sizes_triangulated = new List<UInt32>();
        internal UInt32 number_of_texture_coordinate_indexes = 0;
        internal UInt32 behaviour = PRC_GRAPHICS_Show;
    }

    public class PRCContentBase
    {
        private static UInt32 cadID = 0;
        private static UInt32 prcID = 0;

        internal string name = string.Empty;
        internal readonly UInt32 CAD_identifier = cadID++;
        internal UInt32 CAD_persistent_identifier = 0;
        internal readonly UInt32 PRC_unique_identifier = prcID++;
    }

    public class PRCMaterial : PRCContentBase
    {
        internal UInt32 picture_index = 0;
        internal UInt32 ambient;
        internal UInt32 diffuse;
        internal UInt32 emissive;
        internal UInt32 specular;
        internal double shininess;
        internal double ambient_alpha;
        internal double diffuse_alpha;
        internal double emissive_alpha;
        internal double specular_alpha;
    }

    public class PRCPolyBrepModel : PRCContentBase
    {
        internal bool is_closed = false;
        internal UInt32 index_local_coordinate_system = UInt32.MaxValue;
        internal UInt32 index_tessellation = UInt32.MaxValue;
    }

    public class PRCTransform
    {
        public readonly double[,] mat = new double[4, 4];
    }

    public class PRCFileStructure
    {
        internal readonly List<PRCPolyBrepModel> polymodels = new List<PRCPolyBrepModel>();
        internal uint GetStructureInformationSize()
        {
            throw new NotImplementedException();
        }
    }

    public struct PRCRgbColor
    {
        public double R;
        public double G;
        public double B;
    }

    struct PRCUniqueId
    {
        static UInt32 count = 0;
        UInt32 id0 = 0;
        UInt32 id1 = 0;
        UInt32 id2 = 0;
        UInt32 id3 = 0;

        public static PRCUniqueId MakeFileUUID()
        {
            return new PRCUniqueId()
            {
                id0 = 0x33595341,
                id1 = (UInt32)DateTime.Now.Ticks,
                id2 = count++,
                id3 = (UInt32)new Random().Next(),
            };
        }
    }

    class PRCStartHeader
    {
        internal UInt32 minimal_version_for_read = 8137; // PRCVersion
        internal UInt32 authoring_version = 8137;    // PRCVersion
        internal PRCUniqueId file_structure_uuid = PRCUniqueId.MakeFileUUID();
        internal PRCUniqueId application_uuid = new PRCUniqueId(); // should be 0

        protected UInt32 GetStartHeaderSize()
        {
            return 3 + (2 + 2 * 4) * sizeof(UInt32);
        }
    };

    class PRCHeader : PRCStartHeader
    {
        internal readonly List<PRCFileStructure> file_structure_information = new List<PRCFileStructure>();
        internal UInt32 model_file_offset;
        internal UInt32 file_size; // ??? 

        public UInt32 getSize()
        {
            UInt32 size = GetStartHeaderSize() + sizeof(UInt32);
            foreach (var fileStructure in file_structure_information)
                size += fileStructure.GetStructureInformationSize();

            size += 3 * sizeof(UInt32);

            //foreach (var uncompressedFile in uncompressed_files)
            //    size += uncompressedFile.GetSize();

            return size;
        }
    }

    class PRCUnit
    {
        public bool unit_from_CAD_file;
        public double unit;
    }

    public class PRCFile : PRCContentBase
    {
        internal readonly PRCHeader _header = new PRCHeader();

        internal readonly List<PRCTransform> _transforms = new List<PRCTransform>();
        internal readonly List<PRCRgbColor> _colors = new List<PRCRgbColor>();
        internal readonly List<PRCMaterial> _materials = new List<PRCMaterial>();
        internal readonly List<PRC3DTess> _tessellations = new List<PRC3DTess>();

        internal readonly List<PRCFileStructure> _fileStructures = new List<PRCFileStructure>();
        internal PRCUnit unit;

        private UInt32 addColor(PRCRgbColor color)
        {
            _colors.Add(color);
            return (UInt32)(_colors.Count - 1) * 3;
        }

        public UInt32 addMaterial(PRCRgbColor color)
        {
            var index = addColor(color);
            var material = new PRCMaterial()
            {
                name = "",
                ambient = index,
                diffuse = index,
                emissive = index,
                specular = index,
                ambient_alpha = 1,
                diffuse_alpha = 1,
                emissive_alpha = 1,
                specular_alpha = 1,
                shininess = 1,
            };

            _materials.Add(material);
            return (UInt32)_materials.Count - 1;
        }

        public UInt32 createTriangleMesh(
            Tuple<double, double, double>[] P,
            Tuple<UInt32, UInt32, UInt32>[] PI,
            Tuple<double, double, double>[] N,
            Tuple<UInt32, UInt32, UInt32>[] NI)
        {
            if (PI.Length != NI.Length)
                throw new InvalidOperationException();

            var tess = new PRC3DTess();
            var tessFace = new PRCTessFace();

            tessFace.used_entities_flag = PRCTessFace.PRC_FACETESSDATA_Triangle;
            tessFace.number_of_texture_coordinate_indexes = 0;

            foreach (var p in P)
            {
                tess.coordinates.Add(p.Item1);
                tess.coordinates.Add(p.Item2);
                tess.coordinates.Add(p.Item3);
            }

            foreach (var p in N)
            {
                tess.normal_coordinate.Add(p.Item1);
                tess.normal_coordinate.Add(p.Item2);
                tess.normal_coordinate.Add(p.Item3);
            }

            for (int i = 0; i < PI.Length; i++)
            {
                tess.triangulated_index.Add(3 * NI[i].Item1);
                tess.triangulated_index.Add(3 * PI[i].Item1);

                tess.triangulated_index.Add(3 * NI[i].Item2);
                tess.triangulated_index.Add(3 * PI[i].Item2);

                tess.triangulated_index.Add(3 * NI[i].Item3);
                tess.triangulated_index.Add(3 * PI[i].Item3);
            }

            tessFace.sizes_triangulated.Add((UInt32)PI.Length);
            tess.face_tessellation.Add(tessFace);
            _tessellations.Add(tess);

            return (UInt32)(_tessellations.Count - 1);
        }

        internal uint GetSize()
        {
            throw new NotImplementedException();
        }
    }
}
