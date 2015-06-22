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

        public void Write(string value)
        {
            _streamWriter.Write(value);
        }

        public void Write(bool value)
        {
            _streamWriter.Write(value);
        }

        public void Write(double value)
        {
            _streamWriter.Write(value);
        }

        public void Write(UInt32 value)
        {
            _streamWriter.Write(value);
        }

        public void Write(List<UInt32> list)
        {
            UInt32 size = (UInt32)list.Count;
            _streamWriter.Write(size);
            foreach (var value in list)
                _streamWriter.Write(value);
        }

        public void Write(List<double> list)
        {
            UInt32 size = (UInt32)list.Count;
            _streamWriter.Write(size);
            foreach (var value in list)
                _streamWriter.Write(value);
        }

        public void Write(List<PRCTessFace> list)
        {
            UInt32 size = (UInt32)list.Count;
            _streamWriter.Write(size);
            foreach (var value in list)
                value.Serialize(this);
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

        public const UInt32 PRC_TYPE_GRAPH = (700);
        public const UInt32 PRC_TYPE_GRAPH_Material = (PRC_TYPE_GRAPH + 2);
    }

    public class PRC3DTess
    {
        internal bool _isCalcuated = false;
        internal readonly List<double> _coordinates = new List<double>();

        internal bool _hasFaces = false;
        internal bool _hasLoops = false;

        internal readonly List<double> _normalCoordinate = new List<double>();
        internal readonly List<UInt32> _wireIndex = new List<UInt32>();
        internal readonly List<UInt32> _triangulatedIndex = new List<UInt32>();

        internal readonly List<PRCTessFace> _faceTessellation = new List<PRCTessFace>();

        internal readonly List<double> _textureCoordinate = new List<double>();

        public void Serialize(PRCbitStreamWriter outStream)
        {
            outStream.Write(PRCType.PRC_TYPE_TESS_3D);
            outStream.Write(_isCalcuated);
            outStream.Write(_coordinates);
            outStream.Write(_hasFaces);
            outStream.Write(_hasLoops);
            outStream.Write(false); //must recalculate normals
            outStream.Write(_normalCoordinate);
            outStream.Write(_wireIndex);
            outStream.Write(_triangulatedIndex);
            outStream.Write(_faceTessellation);
            outStream.Write(_textureCoordinate);
        }
    }

    public class PRCTessFace
    {
        internal const UInt32 PRC_GRAPHICS_Show = 0x0001;
        internal const UInt32 PRC_FACETESSDATA_Triangle = 0x0002;

        internal readonly List<UInt32> _lineAttributes = new List<UInt32>();
        internal UInt32 _startWire = 0;
        internal readonly List<UInt32> _sizesWire = new List<UInt32>();
        internal UInt32 _usedEntitiesFlag = 0;
        internal UInt32 _startTriangulated = 0;
        internal readonly List<UInt32> _sizesTriangulated = new List<UInt32>();
        internal UInt32 _numberOfTextureCoordinateIndexes = 0;

        internal UInt32 _behaviour = PRC_GRAPHICS_Show;

        public void Serialize(PRCbitStreamWriter outStream)
        {
            outStream.Write(PRCType.PRC_TYPE_TESS_Face);
            outStream.Write(_lineAttributes);
            outStream.Write(_startWire);
            outStream.Write(_sizesWire);
            outStream.Write(_usedEntitiesFlag);
            outStream.Write(_startTriangulated);
            outStream.Write(_sizesTriangulated);

            outStream.Write(_numberOfTextureCoordinateIndexes);

            outStream.Write(false); //HasVertexColors = False
            if (_lineAttributes.Count > 0)
                outStream.Write(_behaviour);
        }
    }

    public class PRCContentBase
    {
        private static UInt32 cadID = 0;
        private static UInt32 prcID = 0;

        internal string name = string.Empty;
        internal readonly UInt32 CAD_identifier = cadID++;
        internal UInt32 CAD_persistent_identifier = 0;
        internal readonly UInt32 PRC_unique_identifier = prcID;

        public void Serialize(PRCbitStreamWriter outStream)
        {
            outStream.Write(name);
            outStream.Write(CAD_identifier);
            outStream.Write(CAD_persistent_identifier);
            outStream.Write(PRC_unique_identifier);
        }
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

        public void Serialize(PRCbitStreamWriter outStream)
        {
            outStream.Write(PRCType.PRC_TYPE_GRAPH_Material);
            outStream.Write((UInt32)0); //Attributes

            base.Serialize(outStream);

            outStream.Write(ambient + 1);
            outStream.Write(diffuse + 1);
            outStream.Write(emissive + 1);
            outStream.Write(specular + 1);
            outStream.Write(shininess);
            outStream.Write(ambient_alpha);
            outStream.Write(diffuse_alpha);
            outStream.Write(emissive_alpha);
            outStream.Write(specular_alpha);
        }
    }

    public struct PRCRgbColor
    {
        public double R;
        public double G;
        public double B;
    }

    public class PRCPolyBrepModel : PRCContentBase
    {
        const UInt32 m1 = UInt32.MaxValue;

        internal bool _isClosed = false;

        UInt32 index_local_coordinate_system = m1;
        UInt32 index_tessellation = m1;

        UInt32 layer_index = m1;
        UInt32 index_of_line_style = m1;
        UInt32 behaviour_bit_field = m1;

        public void Serialize(PRCbitStreamWriter outStream)
        {

        }
    }

    public class PRCTransform
    {
        public readonly double[,] mat = new double[4, 4];
    }

    public class PRCGroup
    {
        internal readonly List<PRCPolyBrepModel> polymodels = new List<PRCPolyBrepModel>();

        public void Serialize(PRCbitStreamWriter outStream)
        {
            //Todo
        }
    }

    public class PRCFile
    {
        private readonly List<PRCTransform> _transforms = new List<PRCTransform>();
        private readonly List<PRCRgbColor> _colors = new List<PRCRgbColor>();
        private readonly List<PRCMaterial> _materials = new List<PRCMaterial>();
        private readonly List<PRC3DTess> _tessellations = new List<PRC3DTess>();

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

        public void BeginGroup(string name, double[] transform = null)
        {
            //Todo
        }

        public void UseMesh(UInt32 tessIndex, UInt32 materialIndex)
        {
            //Todo
        }

        public void EndGroup()
        {
            //Todo
        }

        public void Serialize(PRCbitStreamWriter outStream)
        {
            //Todo
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

            tessFace._usedEntitiesFlag = PRCTessFace.PRC_FACETESSDATA_Triangle;
            tessFace._numberOfTextureCoordinateIndexes = 0;

            foreach (var p in P)
            {
                tess._coordinates.Add(p.Item1);
                tess._coordinates.Add(p.Item2);
                tess._coordinates.Add(p.Item3);
            }

            foreach (var p in N)
            {
                tess._normalCoordinate.Add(p.Item1);
                tess._normalCoordinate.Add(p.Item2);
                tess._normalCoordinate.Add(p.Item3);
            }

            for (int i = 0; i < PI.Length; i++)
            {
                tess._triangulatedIndex.Add(3 * NI[i].Item1);
                tess._triangulatedIndex.Add(3 * PI[i].Item1);

                tess._triangulatedIndex.Add(3 * NI[i].Item2);
                tess._triangulatedIndex.Add(3 * PI[i].Item2);

                tess._triangulatedIndex.Add(3 * NI[i].Item3);
                tess._triangulatedIndex.Add(3 * PI[i].Item3);
            }

            tessFace._sizesTriangulated.Add((UInt32)PI.Length);
            tess._faceTessellation.Add(tessFace);
            _tessellations.Add(tess);

            return (UInt32)(_tessellations.Count - 1);
        }
    }
}
