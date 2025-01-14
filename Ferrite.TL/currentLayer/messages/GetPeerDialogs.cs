/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Data;
using Ferrite.Data.Messages;
using Ferrite.Services;
using Ferrite.TL.currentLayer.updates;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;

namespace Ferrite.TL.currentLayer.messages;
public class GetPeerDialogs : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IMessagesService _messages;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetPeerDialogs(ITLObjectFactory objectFactory, IMessagesService messages,
        IMapperContext mapper)
    {
        factory = objectFactory;
        _messages = messages;
        _mapper = mapper;
    }

    public int Constructor => -462373635;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peers.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Vector<InputDialogPeer> _peers;
    public Vector<InputDialogPeer> Peers
    {
        get => _peers;
        set
        {
            serialized = false;
            _peers = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        List<InputDialogPeerDTO> peers = new();
        foreach (var p in _peers)
        {
            peers.Add(_mapper.MapToDTO<InputDialogPeer, InputDialogPeerDTO>(p));
        }

        var serviceResult = await _messages.GetPeerDialogs(ctx.CurrentAuthKeyId, peers);
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        if (!serviceResult.Success || serviceResult.Result == null)
        {
            var err = factory.Resolve<RpcError>();
            err.ErrorCode = serviceResult.ErrorMessage.Code;
            err.ErrorMessage = serviceResult.ErrorMessage.Message;
        }

        var dialogs = _mapper.MapToTLObject<PeerDialogs, PeerDialogsDTO>(serviceResult.Result!);
        
        result.Result = dialogs;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _peers  =  factory . Read < Vector < InputDialogPeer > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}