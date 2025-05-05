using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace Fragmenta.Tests;

public static class PlaywrightHelpers
{
    public static async Task DragAndDrop(this IPage page, string sourceSelector, string targetSelector)
    {
        await page.Locator(sourceSelector).HoverAsync();
        await page.Mouse.DownAsync();
        await page.Locator(targetSelector).HoverAsync();
        await page.Mouse.UpAsync();
    }
    
    public static async Task<bool> ElementExists(this IPage page, string selector, int timeoutMs = 1000)
    {
        try
        {
            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
            { 
                Timeout = timeoutMs,
                State = WaitForSelectorState.Attached
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
    
    public static async Task<string> GetTextContent(this IPage page, string selector)
    {
        return await page.Locator(selector).TextContentAsync() ?? string.Empty;
    }
    
    public static async Task<int> GetElementCount(this IPage page, string selector)
    {
        return await page.Locator(selector).CountAsync();
    }

    public static async Task<List<string>> GetAllTextContents(this IPage page, string selector)
    {
        var elements = page.Locator(selector);
        var count = await elements.CountAsync();
        var result = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            var text = await elements.Nth(i).TextContentAsync();
            result.Add(text ?? string.Empty);
        }
        
        return result;
    }
    
    /// <summary>
    /// Check the hierarchical relationship between two elements
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <param name="elementSelector">First element selector</param>
    /// <param name="otherElementSelector">Second element selector</param>
    /// <returns>
    /// 1: First element is inside second element
    /// 0: Elements are at the same level
    /// -1: First element is outside second element
    /// </returns>
    public static async Task<int> CheckElementRelationship(this IPage page, string elementSelector, string otherElementSelector)
    {
        // This JavaScript evaluates the DOM relationship between the elements
        var script = @"(elementSelector, otherElementSelector) => {
            const element = document.querySelector(elementSelector);
            const otherElement = document.querySelector(otherElementSelector);
            
            if (!element || !otherElement) return null;
            
            // Check if element is inside otherElement
            if (otherElement.contains(element) && element !== otherElement) {
                return 1;
            }
            
            // Check if otherElement is inside element
            if (element.contains(otherElement) && element !== otherElement) {
                return -1;
            }
            
            // Check if they're at the same level (siblings or unrelated but same level)
            return 0;
        }";
        
        var result = await page.EvaluateAsync<int?>(script, new[] {elementSelector, otherElementSelector });
        return result ?? 0; // Default to 0 if elements weren't found
    }
}