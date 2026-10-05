
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Util;
using NUnit.Framework;
using RandomizedTesting.Generators;
using System;

namespace Lucene.Net.Codecs.Lucene40
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    using LuceneTestCase = Lucene.Net.Util.LuceneTestCase;
    using TestUtil = Lucene.Net.Util.TestUtil;

    [TestFixture]
    public class TestBitVectorBackCompat : LuceneTestCase
    {
        static BitVector_3_0_3 NewBitVector(int size)
        {
            return new BitVector_3_0_3(size);
        }

        [Test]
        public void TestRandom()
        {
            for (var i = 0; i < 50; i++)
            {
                DoTestRandom();
            }
        }

        private void DoTestRandom()
        {
            // set random bits, of some sparsity
            int size = TestUtil.NextInt32(Random, 1, 100_000);
            int numSet = Random.nextInt(size);

            BitVector_3_0_3 bv = NewBitVector(size);
            if (numSet == size) {
                for (int i = 0; i < size; i++) {
                    bv.Set(i);
                }
            } else {
                for (int i = 0; i < numSet; i++) {
                    while (true) {
                        int o = Random.nextInt(size);
                        if (!bv.Get(o)) {
                            bv.Set(o);
                            break;
                        }
                    }
                }
            }

            // serialize to ramdir
            RAMDirectory ramdir = new RAMDirectory();
            bv.Write(ramdir, "bits");

            // read back with current code
            BitVector current = new BitVector(ramdir, "bits", IOContext.DEFAULT);

            assertEquals(size, current.Length);
            assertEquals(numSet, current.Length - current.Count());
            for (int i = 0; i < size; i++) {
                // we must assert they are opposites: because we look at "live docs" but these wrote "deleted docs"
                assertEquals(bv.Get(i), !current.Get(i));
            }

            ramdir.Dispose();
        }
    }
}
