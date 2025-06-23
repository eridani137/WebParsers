using System.Collections.Immutable;
using ActivityChecker.IO;
using Microsoft.Extensions.Logging;
using Shared;
using Spectre.Console;
using VkNet;

namespace ActivityChecker.Parsers;

public class VkParser(VkApi vkApi, ILogger<VkParser> logger) : ISiteParser
{
    public string Url => "https://vk.com";

    public async Task<List<ViewResult>> GetViewCount(ImmutableList<string> lines)
    {
        var result = new List<ViewResult>();
        
        var batches = lines.SplitIntoBatches(100);

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("Получение постов...", maxValue: lines.Count);
                
                foreach (var batch in batches)
                {
                    var wallObject = await vkApi.Wall.GetByIdAsync(batch.Select(u => u.Replace($"{Url}/wall", "")), true);
                    foreach (var post in wallObject.WallPosts)
                    {
                        try
                        {
                            var views = post.Views?.Count ?? 0;
                            var endWith = $"{post.OwnerId}_{post.Id}";

                            result.Add(new ViewResult()
                            {
                                Url = batch.First(s => s.EndsWith(endWith)),
                                Views = views
                            });
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Ошибка в цикле обработки батчей");
                        }
                        finally
                        {
                            task.Increment(1);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });

        return result;
    }
}