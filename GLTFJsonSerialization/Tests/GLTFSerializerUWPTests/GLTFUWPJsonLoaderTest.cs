﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GLTFJsonSerialization;
using GLTFJsonSerializerTests;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;


namespace GLTFJsonSerializerUWPTests
{
    [TestClass]
    public class GLTFUWPJsonLoaderTest
    {
        readonly string GLTF_PATH = @"ms-appx:///Assets/glTF/BoomBox.gltf";

        [TestMethod]
        public async Task LoadGLTFFromStreamUWP()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(GLTF_PATH));


            IRandomAccessStream gltfStream = await sampleFile.OpenAsync(FileAccessMode.Read);
            GLTFRoot gltfRoot = GLTFParser.ParseJson(gltfStream.AsStream());
            GLTFJsonLoadTestHelper.TestGLTF(gltfRoot);
        }
    }
}
