// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Ferrite.TLParser;
using Xunit;

namespace Ferrite.Tests.TL;

public class TLSourceGeneratorTests
{
    [Fact]
    public void TLSourceGenerator_Should_Generate_Source_WithVectorBareProperties()
    {
        string source = @"future_salts#ae500895 req_msg_id:long now:int salts:vector<future_salt> = FutureSalts;
";
        TLSourceGenerator generator = new TLSourceGenerator();
        var generated = generator.Generate("mtproto", source).First();
        Assert.Contains("public future_salts(long req_msg_id, int now, VectorBare salts)", generated.SourceText);
        Assert.Contains("public VectorBare salts => new VectorBare(_buff.Slice(GetOffset(3, _buff)));", generated.SourceText);
        Assert.Contains("if(index >= 4) offset += VectorBare.ReadSize(buffer, offset);", generated.SourceText);
        Assert.Contains("public TLObjectBuilder with_salts(VectorBare value)", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_GenerateCorrectNamespaceForCombinator()
    {
        string source = @"storage.fileJpeg#7efe0e = storage.FileType;
";
        TLSourceGenerator generator = new TLSourceGenerator();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("namespace Ferrite.TL.slim.layer146.storage;", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_GenerateCorrectNamespaceForFunction()
    {
        string source = @"---functions---
help.getConfig#c4f9186b = Config;
";
        TLSourceGenerator generator = new TLSourceGenerator();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("namespace Ferrite.TL.slim.layer146.help;", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_GenerateObjectReader()
    {
        string source = @"---functions---
help.getConfig#c4f9186b = Config;
";
        TLSourceGenerator generator = new TLSourceGenerator();
        var generated = generator.Generate("layer146", source).First();
        var objectReader = generator.GenerateObjectReader();
        Assert.Contains("using Ferrite.TL.slim.layer146.help;", objectReader.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_inputMediaPoll()
    {
        string source = @"inputMediaPoll#f94e5f1 flags:# poll:Poll correct_answers:flags.0?Vector<bytes> solution:flags.1?string solution_entities:flags.1?Vector<MessageEntity> = InputMedia;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public inputMediaPoll(Flags flags, ReadOnlySpan<byte> poll, Vector correct_answers, ReadOnlySpan<byte> solution, Vector solution_entities)", generated.SourceText);
        Assert.Contains("Set_flags(flags);", generated.SourceText);
        Assert.Contains(@"if(flags[1])
        {
            Set_solution_entities(solution_entities.ToReadOnlySpan());
        }", generated.SourceText);
        Assert.Contains("public readonly Flags flags => new Flags(MemoryMarshal.Read<int>(_buff[GetOffset(1, _buff)..]));", generated.SourceText);
        Assert.Contains(" var bufferLength = GetRequiredBufferSize(poll.Length, (flags[0]?correct_answers.Length:0), flags[1], solution.Length, (flags[1]?solution_entities.Length:0));", generated.SourceText);
        Assert.Contains(@"public static int GetRequiredBufferSize(int len_poll, int len_correct_answers, bool has_solution, int len_solution, int len_solution_entities)
    {
        return 4 + 4 + len_poll + len_correct_answers + (has_solution?BufferUtils.CalculateTLBytesLength(len_solution):0) + len_solution_entities;
    }", generated.SourceText);
        Assert.Contains("public Vector correct_answers => !flags[0] ? new Vector() : new Vector(_buff.Slice(GetOffset(3, _buff)));", generated.SourceText);
        Assert.Contains("public ReadOnlySpan<byte> solution => !flags[1] ? new ReadOnlySpan<byte>() :  BufferUtils.GetTLBytes(_buff, GetOffset(4, _buff));", generated.SourceText);
        Assert.Contains("private Flags _flags = new Flags();", generated.SourceText);
        Assert.Contains(@"public TLObjectBuilder with_solution(ReadOnlySpan<byte> value)
        {
            _solution = value;
            _flags[1] = true;
            return this;
        }", generated.SourceText);
        Assert.Contains("return new inputMediaPoll(_flags, _poll, _correct_answers, _solution, _solution_entities);", generated.SourceText);
    }

    [Fact]
    public void TLSourceGenerator_Should_Generate_inputGeoPoint()
    {
        string source = @"inputGeoPoint#48222faf flags:# lat:double long:double accuracy_radius:flags.0?int = InputGeoPoint;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public readonly int accuracy_radius => !flags[0] ? 0 : MemoryMarshal.Read<int>(_buff[GetOffset(4, _buff)..]);", generated.SourceText);
        Assert.Contains("public readonly double longitude => MemoryMarshal.Read<double>(_buff[GetOffset(3, _buff)..]);", generated.SourceText);
        Assert.Contains("var bufferLength = GetRequiredBufferSize(flags[0]);", generated.SourceText);
        Assert.Contains("public static int GetRequiredBufferSize(bool has_accuracy_radius)", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_inputMediaInvoice()
    {
        string source = @"inputMediaInvoice#8eb5a6d5 flags:# title:string description:string photo:flags.0?InputWebDocument invoice:Invoice payload:bytes provider:string provider_data:DataJSON start_param:flags.1?string extended_media:flags.2?InputMedia = InputMedia;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("ReadOnlySpan<byte> start_param, ReadOnlySpan<byte> extended_media)", generated.SourceText);
        Assert.Contains("var bufferLength = GetRequiredBufferSize(title.Length, description.Length, (flags[0]?photo.Length:0), invoice.Length, payload.Length, provider.Length, provider_data.Length, flags[1], start_param.Length, (flags[2]?extended_media.Length:0));", generated.SourceText);
        Assert.Contains("Set_photo(photo);", generated.SourceText);
        Assert.Contains("public Span<byte> photo => !flags[0] ? new Span<byte>() : ObjectReader.Read(_buff);", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_channelFull()
    {
        string source = @"channelFull#f2355507 flags:# can_view_participants:flags.3?true can_set_username:flags.6?true can_set_stickers:flags.7?true hidden_prehistory:flags.10?true can_set_location:flags.16?true has_scheduled:flags.19?true can_view_stats:flags.20?true blocked:flags.22?true flags2:# can_delete_channel:flags2.0?true id:long about:string participants_count:flags.0?int admins_count:flags.1?int kicked_count:flags.2?int banned_count:flags.2?int online_count:flags.13?int read_inbox_max_id:int read_outbox_max_id:int unread_count:int chat_photo:Photo notify_settings:PeerNotifySettings exported_invite:flags.23?ExportedChatInvite bot_info:Vector<BotInfo> migrated_from_chat_id:flags.4?long migrated_from_max_id:flags.4?int pinned_msg_id:flags.5?int stickerset:flags.8?StickerSet available_min_id:flags.9?int folder_id:flags.11?int linked_chat_id:flags.14?long location:flags.15?ChannelLocation slowmode_seconds:flags.17?int slowmode_next_send_date:flags.18?int stats_dc:flags.12?int pts:int call:flags.21?InputGroupCall ttl_period:flags.24?int pending_suggestions:flags.25?Vector<string> groupcall_default_join_as:flags.26?Peer theme_emoticon:flags.27?string requests_pending:flags.28?int recent_requesters:flags.28?Vector<long> default_send_as:flags.29?Peer available_reactions:flags.30?ChatReactions = ChatFull;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public readonly bool can_delete_channel => flags2[0];", generated.SourceText);
        Assert.Contains(@"private Flags _flags2 = new Flags();
        public TLObjectBuilder with_can_delete_channel(bool value)
        {
            _flags2[0] = value;
            return this;
        }", generated.SourceText);
        Assert.Contains("Set_flags(flags);", generated.SourceText);
        Assert.Contains(@"if(flags[4])
        {
            Set_migrated_from_max_id(migrated_from_max_id);
        }", generated.SourceText);
        Assert.Contains("var bufferLength = GetRequiredBufferSize(hidden_prehistory.Length, can_set_location.Length, has_scheduled.Length, can_view_stats.Length, blocked.Length, about.Length, flags[0], flags[1], flags[2], flags[2], online_count.Length, chat_photo.Length, notify_settings.Length, exported_invite.Length, bot_info.Length, flags[4], flags[4], flags[5], (flags[8]?stickerset.Length:0), flags[9], folder_id.Length, linked_chat_id.Length, location.Length, slowmode_seconds.Length, slowmode_next_send_date.Length, stats_dc.Length, call.Length, ttl_period.Length, pending_suggestions.Length, groupcall_default_join_as.Length, theme_emoticon.Length, requests_pending.Length, recent_requesters.Length, default_send_as.Length, available_reactions.Length);", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_message()
    {
        string source = @"message#38116ee0 flags:# out:flags.1?true mentioned:flags.4?true media_unread:flags.5?true silent:flags.13?true post:flags.14?true from_scheduled:flags.18?true legacy:flags.19?true edit_hide:flags.21?true pinned:flags.24?true noforwards:flags.26?true id:int from_id:flags.8?Peer peer_id:Peer fwd_from:flags.2?MessageFwdHeader via_bot_id:flags.11?long reply_to:flags.3?MessageReplyHeader date:int message:string media:flags.9?MessageMedia reply_markup:flags.6?ReplyMarkup entities:flags.7?Vector<MessageEntity> views:flags.10?int forwards:flags.10?int replies:flags.23?MessageReplies edit_date:flags.15?int post_author:flags.16?string grouped_id:flags.17?long reactions:flags.20?MessageReactions restriction_reason:flags.22?Vector<RestrictionReason> ttl_period:flags.25?int = Message;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("Set_message_(message_);", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_messageMediaInvoice()
    {
        string source = @"messageMediaInvoice#f6a548d3 flags:# shipping_address_requested:flags.1?true test:flags.3?true title:string description:string photo:flags.0?WebDocument receipt_msg_id:flags.2?int currency:string total_amount:long start_param:string extended_media:flags.4?MessageExtendedMedia = MessageMedia;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("return new messageMediaInvoice(_flags, _flags[1], _flags[3], _title, _description, _photo, _receipt_msg_id, _currency, _total_amount, _start_param, _extended_media);", generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_auth_sentCode()
    {
        string source = @"auth.sentCode#5e002502 flags:# type:auth.SentCodeType phone_code_hash:string next_type:flags.1?auth.CodeType timeout:flags.2?int = auth.SentCode;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("GetRequiredBufferSize(type.Length, phone_code_hash.Length, (flags[1]?next_type.Length:0), flags[2])",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_updates()
    {
        string source = @"
updates#74ae4240 updates:Vector<Update> users:Vector<User> chats:Vector<Chat> date:int seq:int = Updates;
updates.differenceEmpty#5d75a138 date:int seq:int = updates.Difference;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public readonly ref struct updates_",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_chatFull()
    {
        string source = @"
messages.chatFull#e5d7d19c full_chat:ChatFull chats:Vector<Chat> users:Vector<User> = messages.ChatFull;
chatFull#c9d31138 flags:# can_set_username:flags.7?true has_scheduled:flags.8?true id:long about:string participants:ChatParticipants chat_photo:flags.2?Photo notify_settings:PeerNotifySettings exported_invite:flags.13?ExportedChatInvite bot_info:flags.3?Vector<BotInfo> pinned_msg_id:flags.6?int folder_id:flags.11?int call:flags.12?InputGroupCall ttl_period:flags.14?int groupcall_default_join_as:flags.15?Peer theme_emoticon:flags.16?string requests_pending:flags.17?int recent_requesters:flags.17?Vector<long> available_reactions:flags.18?ChatReactions = ChatFull;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public readonly ref struct messages_chatFull",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_inputPeerNotifySettings()
    {
        string source = @"inputPeerNotifySettings#df1f002b flags:# show_previews:flags.0?Bool silent:flags.1?Bool mute_until:flags.2?int sound:flags.3?NotificationSound = InputPeerNotifySettings;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains(@"public static int GetRequiredBufferSize(bool has_show_previews, bool has_silent, bool has_mute_until, int len_sound)
    {
        return 4 + 4 + (has_show_previews?4:0) + (has_silent?4:0) + (has_mute_until?4:0) + len_sound;
    }",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_config()
    {
        string source = @"config#232566ac flags:# phonecalls_enabled:flags.1?true default_p2p_contacts:flags.3?true preload_featured_stickers:flags.4?true ignore_phone_entities:flags.5?true revoke_pm_inbox:flags.6?true blocked_mode:flags.8?true pfs_enabled:flags.13?true force_try_ipv6:flags.14?true date:int expires:int test_mode:Bool this_dc:int dc_options:Vector<DcOption> dc_txt_domain_name:string chat_size_max:int megagroup_size_max:int forwarded_count_max:int online_update_period_ms:int offline_blur_timeout_ms:int offline_idle_timeout_ms:int online_cloud_timeout_ms:int notify_cloud_delay_ms:int notify_default_delay_ms:int push_chat_period_ms:int push_chat_limit:int saved_gifs_limit:int edit_time_limit:int revoke_time_limit:int revoke_pm_time_limit:int rating_e_decay:int stickers_recent_limit:int stickers_faved_limit:int channels_read_media_period:int tmp_sessions:flags.0?int pinned_dialogs_count_max:int pinned_infolder_count_max:int call_receive_timeout_ms:int call_ring_timeout_ms:int call_connect_timeout_ms:int call_packet_timeout_ms:int me_url_prefix:string autoupdate_url_prefix:flags.7?string gif_search_username:flags.9?string venue_search_username:flags.10?string img_search_username:flags.11?string static_maps_provider:flags.12?string caption_length_max:int message_length_max:int webfile_dc_id:int suggested_lang_code:flags.2?string lang_pack_version:flags.2?int base_lang_pack_version:flags.2?int reactions_default:flags.15?Reaction = Config;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains(", bool test_mode,",generated.SourceText);
        Assert.Contains("GetRequiredBufferSize(pfs_enabled.Length, force_try_ipv6.Length, dc_options.Length, dc_txt_domain_name.Length, flags[0], me_url_prefix.Length, flags[7], autoupdate_url_prefix.Length, flags[9], gif_search_username.Length, venue_search_username.Length, img_search_username.Length, static_maps_provider.Length, flags[2], suggested_lang_code.Length, flags[2], flags[2], reactions_default.Length)",generated.SourceText);
        Assert.Contains(@"public readonly bool test_mode => MemoryMarshal.Read<int>(_buff[GetOffset(6, _buff)..]) == unchecked((int)0x997275b5);
    private void Set_test_mode(bool value)
    {
        int t = unchecked((int)0x997275b5);
        int f = unchecked((int)0xbc799737);
        if(value)
        {
            MemoryMarshal.Write(_buff[GetOffset(6, _buff)..], ref t);
        }
        else 
        {
            MemoryMarshal.Write(_buff[GetOffset(6, _buff)..], ref f);
        }
    }",generated.SourceText);
        Assert.Contains(@"private bool _test_mode;
        public TLObjectBuilder with_test_mode(bool value)
        {
            _test_mode = value;
            return this;
        }",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_auth_sendCode()
    {
        string source = @"
---functions---
auth.sendCode#a677244f phone_number:string api_id:int api_hash:string settings:CodeSettings = auth.SentCode;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public sendCode(ReadOnlySpan<byte> phone_number, int api_id, ReadOnlySpan<byte> api_hash, ReadOnlySpan<byte> settings)",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_auth_initConnection()
    {
        string source = @"
---functions---
initConnection#c1cd5ea9 {X:Type} flags:# api_id:int device_model:string system_version:string app_version:string system_lang_code:string lang_pack:string lang_code:string proxy:flags.0?InputClientProxy params:flags.1?JSONValue query:!X = X;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public Span<byte> query => ObjectReader.Read(_buff);",generated.SourceText);
        Assert.Contains("if(index >= 12) offset += ObjectReader.ReadSize(buffer[offset..]);",generated.SourceText);
    }
    [Fact]
    public void TLSourceGenerator_Should_Generate_Functions()
    {
        string source = @"

messageExtendedMediaPreview#ad628cc8 flags:# w:flags.0?int h:flags.0?int thumb:flags.1?PhotoSize video_duration:flags.2?int = MessageExtendedMedia;
messageExtendedMedia#ee479c64 media:MessageMedia = MessageExtendedMedia;

---functions---
invokeAfterMsg#cb9f372d {X:Type} msg_id:long query:!X = X;
invokeAfterMsgs#3dc4b4f0 {X:Type} msg_ids:Vector<long> query:!X = X;
initConnection#c1cd5ea9 {X:Type} flags:# api_id:int device_model:string system_version:string app_version:string system_lang_code:string lang_pack:string lang_code:string proxy:flags.0?InputClientProxy params:flags.1?JSONValue query:!X = X;
invokeWithLayer#da9b0d0d {X:Type} layer:int query:!X = X;
invokeWithoutUpdates#bf9459b7 {X:Type} query:!X = X;
invokeWithMessagesRange#365275f2 {X:Type} range:MessageRange query:!X = X;
invokeWithTakeout#aca9fd2e {X:Type} takeout_id:long query:!X = X;
auth.sendCode#a677244f phone_number:string api_id:int api_hash:string settings:CodeSettings = auth.SentCode;
auth.signUp#80eee427 phone_number:string phone_code_hash:string first_name:string last_name:string = auth.Authorization;
auth.signIn#8d52a951 flags:# phone_number:string phone_code_hash:string phone_code:flags.0?string email_verification:flags.1?EmailVerification = auth.Authorization;
auth.logOut#3e72ba19 = auth.LoggedOut;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).ToList();
        Assert.Equal(14, generated.Count);
    }

    [Fact]
    public void TLSourceGenerator_Should_Generate_account_getAllSecureValues()
    {
        string source = @"
---functions---
account.getAllSecureValues#b288bc7d = Vector<SecureValue>;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public readonly ref struct getAllSecureValues", generated.SourceText);
    }
    [Fact]
        public void TLSourceGenerator_Should_Generate_messages_setInlineBotResults()
        {
            string source = @"
messages.setInlineBotResults#eb5ea206 flags:# gallery:flags.0?true private:flags.1?true query_id:long results:Vector<InputBotInlineResult> cache_time:int next_offset:flags.2?string switch_pm:flags.3?InlineBotSwitchPM = Bool;
    ";
            TLSourceGenerator generator = new();
            var generated = generator.Generate("layer146", source).First();
            Assert.Contains("public setInlineBotResults(Flags flags, bool gallery, bool private_, long query_id,", generated.SourceText);
        }
    [Fact]
    public void TLSourceGenerator_Should_Generate_inputCheckPasswordSRP()
    {
        string source = @"
inputCheckPasswordSRP#d27ff082 srp_id:long A:bytes M1:bytes = InputCheckPasswordSRP;
";
        TLSourceGenerator generator = new();
        var generated = generator.Generate("layer146", source).First();
        Assert.Contains("public inputCheckPasswordSRP(long srp_id, ReadOnlySpan<byte> A, ReadOnlySpan<byte> M1)", generated.SourceText);
    }
}