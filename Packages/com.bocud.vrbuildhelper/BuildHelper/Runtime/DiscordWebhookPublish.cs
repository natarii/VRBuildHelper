﻿using System;
using System.Collections.Generic;
using BocuD.BuildHelper;
using BocuD.VRChatApiTools;
using UnityEngine;
using VRC.Core;

namespace BuildHelper.Runtime
{
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    public class DiscordWebhookPublish
    {
        [Serializable]
        public class DiscordWebhookData
        {
            public string url = "";
            public string username = "VRBuildHelper";
            public string avatarUrl = "";
            
            [TextArea] [Tooltip("Available shortcuts: {WorldName}, {BranchName}, {UploadedPlatforms}")]
            public string message = "{WorldName} on branch {BranchName} was just updated!";

            public Color color;

            public EmbedMode embedMode = EmbedMode.Embed;
            public enum EmbedMode
            {
                Embed,
                NoEmbed
            }
            
            //embed fields
            public bool supportedPlatformsField = false;
            public bool updatedPlatformsField = true;
            public bool branchField = true;
            public bool versionField = false;
            public bool buildNumberField = true;
            public bool sizeField = true;
        }
        
        public static async void SendPublishedMessage(Branch branch, string content = "")
        {
            DiscordWebhookData data = branch.webhookSettings;
            ApiWorld worldData = await VRChatApiTools.FetchApiWorldAsync(branch.blueprintID);

            DiscordMessage message = new DiscordMessage
            {
                username = data.username,
                avatar_url = data.avatarUrl,
            };
            
            content = content == "" ? data.message : content;
            content = content.Replace("{WorldName}", worldData.name).Replace("{BranchName}", branch.name).Replace("{UploadedPlatforms}", branch.buildData.justUploadedPlatforms);

            switch (data.embedMode)
            {
                case DiscordWebhookData.EmbedMode.Embed:
                    DiscordMessage.Embed embed = new DiscordMessage.Embed
                    {
                        title = branch.cachedName,
                        description = content,
                        url = "https://vrchat.com/home/launch?worldId=" + branch.blueprintID,
                        color = (int) data.color.r * 255 << 16 | (int) data.color.g * 255 << 8 | (int) data.color.b * 255,

                        thumbnail = new DiscordMessage.Embed.Thumbnail
                        {
                            url = worldData.thumbnailImageUrl
                        },
                    };
                    
                    List<DiscordMessage.Embed.Field> fields = new List<DiscordMessage.Embed.Field>();
                    if (data.supportedPlatformsField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Supports",
                            value = branch.cachedPlatforms.ToPrettyString(),
                            inline = true
                        });
                    }
                    if (data.updatedPlatformsField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Updated platforms",
                            value = branch.buildData.justUploadedPlatforms,
                            inline = true
                        });
                    }
                    if (data.branchField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Branch",
                            value = branch.name,
                            inline = true
                        });
                    }
                    if (data.versionField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Version",
                            value = worldData.version.ToString(),
                            inline = true
                        });
                    }
                    if (data.buildNumberField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Build Number",
                            value = branch.buildData.GetLatestUpload().buildVersion.ToString(),
                            inline = true
                        });
                    }
                    if (data.sizeField)
                    {
                        fields.Add(new DiscordMessage.Embed.Field
                        {
                            name = "Size",
                            value = branch.buildData.GetLatestUpload().buildSize.ToReadableBytes(),
                            inline = true
                        });
                    }
                    embed.fields = fields;

                    message.embeds.Add(embed);
                    break;
                
                case DiscordWebhookData.EmbedMode.NoEmbed:
                    message.content = content;
                    break;
            }

            message.SendMessage(data.url);
        }
    }
#endif
}