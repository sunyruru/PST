﻿using pst.encodables;
using pst.encodables.ndb;
using pst.impl.decoders.ltp.hn;
using pst.impl.decoders.messaging;
using pst.impl.decoders.ndb;
using pst.impl.decoders.ndb.blocks;
using pst.impl.decoders.ndb.blocks.data;
using pst.impl.ltp;
using pst.impl.ltp.pc;
using pst.impl.ltp.tc;
using pst.impl.messaging;
using pst.impl.messaging.cache;
using pst.interfaces;
using pst.interfaces.ltp;
using pst.interfaces.messaging;
using pst.interfaces.ndb;
using System.IO;

namespace pst
{
    public partial class PSTFile
    {
        private static IPropertyContextBasedComponent CreatePropertyContextBasedComponent(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache,
            ICache<NumericalTaggedPropertyPath, PropertyContextBasedCachedPropertyState> numericalTaggedPropertyCache,
            ICache<StringTaggedPropertyPath, PropertyContextBasedCachedPropertyState> stringTaggedPropertyCache,
            ICache<TaggedPropertyPath, PropertyContextBasedCachedPropertyState> taggedPropertyCache)
        {
            return
                new PropertyContextBasedComponentThatCachesThePropertyValue(
                    numericalTaggedPropertyCache,
                    stringTaggedPropertyCache,
                    taggedPropertyCache,
                    new PropertyContextBasedComponent(
                        CreatePropertyIdToNameMap(dataStream, nodeEntryCache, dataBlockEntryCache),
                        CreatePropertyContextBasedPropertyReader(dataStream, nodeEntryCache, dataBlockEntryCache)));
        }

        private static ITableContextBasedReadOnlyComponent<Tag> CreateTagBasedTableContextBasedReadOnlyComponent(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache,
            ICache<NumericalTaggedPropertyPath, TableContextBasedCachedPropertyState<Tag>> numericalTaggedPropertyCache,
            ICache<StringTaggedPropertyPath, TableContextBasedCachedPropertyState<Tag>> stringTaggedPropertyCache,
            ICache<TaggedPropertyPath, TableContextBasedCachedPropertyState<Tag>> taggedPropertyCache)
        {
            return
                new TableContextBasedReadOnlyComponentThatCachesThePropertyValue<Tag>(
                    numericalTaggedPropertyCache,
                    stringTaggedPropertyCache,
                    taggedPropertyCache,
                    new TableContextBasedReadOnlyComponent<Tag>(
                        CreatePropertyIdToNameMap(dataStream, nodeEntryCache, dataBlockEntryCache),
                        CreateTagBasedTableContextBasedPropertyReader(dataStream, nodeEntryCache, dataBlockEntryCache)));
        }

        private static ITableContextBasedPropertyReader<Tag> CreateTagBasedTableContextBasedPropertyReader(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new TableContextBasedPropertyReader<Tag>(
                    CreateTagBasedRowMatrixReader(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreatePropertyValueProcessor(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyNameToIdMap CreatePropertyIdToNameMap(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyNameToIdMap(
                    new NAMEIDDecoder(),
                    CreatePropertyContextBasedPropertyReader(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyContextBasedPropertyReader CreatePropertyContextBasedPropertyReader(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyContextBasedPropertyReader(
                    CreatePropertyIdBasedBTreeOnHeapReader(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreatePropertyValueProcessor(dataStream, nodeEntryCache, dataBlockEntryCache));
        }

        private static IPropertyValueProcessor CreatePropertyValueProcessor(
            Stream dataStream,
            ICache<NodePath, NodeEntry> nodeEntryCache,
            ICache<BID, DataBlockEntry> dataBlockEntryCache)
        {
            return
                new PropertyValueProcessor(
                    new HNIDDecoder(
                        new HIDDecoder(),
                        new NIDDecoder()),
                    new ExternalDataBlockDecoder(
                        new BlockTrailerDecoder(
                            new BIDDecoder()),
                        CreateBlockEncoding(dataStream)),
                    CreateDataBlockReader(dataStream, dataBlockEntryCache),
                    CreateDataBlockEntryFinder(dataStream, dataBlockEntryCache),
                    CreateNodeEntryFinder(dataStream, nodeEntryCache, dataBlockEntryCache),
                    CreateHeapOnNodeReader(dataStream, nodeEntryCache, dataBlockEntryCache),
                    new PropertyTypeMetadataProvider());
        }
    }
}
