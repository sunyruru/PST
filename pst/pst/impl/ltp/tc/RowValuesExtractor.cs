﻿using pst.interfaces.ltp.tc;
using System.Collections.Generic;
using pst.encodables.ltp.tc;
using pst.utilities;
using System;

namespace pst.impl.ltp.tc
{
    class RowValuesExtractor : IRowValuesExtractor
    {
        public IReadOnlyDictionary<int, BinaryData> Extract(
            BinaryData rowData,
            TCOLDESC[] columnDescriptors)
        {
            var cebSize =
                (int)
                Math.Ceiling((double)columnDescriptors.Length / 8);

            var ceb =
                rowData.TakeAt(rowData.Length - cebSize, cebSize)
                .ToBits();

            var values = new Dictionary<int, BinaryData>();

            for (int i = 0; i < columnDescriptors.Length; i++)
            {
                if (ceb[i] == 1)
                {
                    values.Add(
                        columnDescriptors[i].Tag,
                        rowData.TakeAt(
                            columnDescriptors[i].DataOffset,
                            columnDescriptors[i].DataSize));
                }
            }

            return values;
        }
    }
}