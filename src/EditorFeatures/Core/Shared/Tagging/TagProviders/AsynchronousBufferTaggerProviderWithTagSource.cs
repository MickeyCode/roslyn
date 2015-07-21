// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Editor.Tagging;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.CodeAnalysis.Editor.Shared.Tagging
{
    internal sealed class AsynchronousBufferTaggerProviderWithTagSource<TTag, TState> :
        AbstractAsynchronousTaggerProvider<ProducerPopulatedTagSource<TTag, TState>, TTag>,
        ITaggerProvider
        where TTag : ITag
    {
        private readonly IAsynchronousTaggerDataSource<TTag, TState> _dataSource;
        private readonly CreateTagSource<ProducerPopulatedTagSource<TTag, TState>, TTag> _createTagSource;

        public AsynchronousBufferTaggerProviderWithTagSource(
            IAsynchronousTaggerDataSource<TTag, TState> dataSource,
            IAsynchronousOperationListener asyncListener,
            IForegroundNotificationService notificationService,
            CreateTagSource<ProducerPopulatedTagSource<TTag, TState>, TTag> createTagSource)
            : base(asyncListener, notificationService)
        {
            this._dataSource = dataSource;
            this._createTagSource = createTagSource;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer subjectBuffer) where T : ITag
        {
            if (subjectBuffer == null)
            {
                throw new ArgumentNullException("subjectBuffer");
            }

            return this.GetOrCreateTagger<T>(null, subjectBuffer);
        }

        protected override ProducerPopulatedTagSource<TTag, TState> CreateTagSourceCore(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            var tagSource = _createTagSource == null ? null : _createTagSource(textViewOpt, subjectBuffer, AsyncListener, NotificationService);
            return tagSource ?? new ProducerPopulatedTagSource<TTag, TState>(textViewOpt, subjectBuffer, _dataSource, AsyncListener, NotificationService);
        }

        protected sealed override bool TryRetrieveTagSource(ITextView textViewOpt, ITextBuffer subjectBuffer, out ProducerPopulatedTagSource<TTag, TState> tagSource)
        {
            return subjectBuffer.Properties.TryGetProperty(UniqueKey, out tagSource);
        }

        protected sealed override void StoreTagSource(ITextView textViewOpt, ITextBuffer subjectBuffer, ProducerPopulatedTagSource<TTag, TState> tagSource)
        {
            subjectBuffer.Properties.AddProperty(UniqueKey, tagSource);
        }

        protected sealed override void RemoveTagSource(ITextView textViewOpt, ITextBuffer subjectBuffer)
        {
            subjectBuffer.Properties.RemoveProperty(UniqueKey);
        }
    }
}