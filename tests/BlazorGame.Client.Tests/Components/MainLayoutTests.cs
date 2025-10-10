using Bunit;
using Xunit;
using FluentAssertions;
using BlazorGame.Client.Layout;

namespace BlazorGame.Tests
{
    public class MainLayoutTests : TestContext
    {
        [Fact]
        public void MainLayout_RendersPageDivAndSidebar()
        {
            var cut = RenderComponent<MainLayout>();

            var pageDiv = cut.Find("div.page");
            Assert.NotNull(pageDiv);

            var sidebarDiv = cut.Find("div.sidebar");
            Assert.NotNull(sidebarDiv);

            var navMenu = cut.FindComponent<NavMenu>();
            Assert.NotNull(navMenu);
        }


        [Fact]
        public void MainLayout_RendersMainAndArticleForBody()
        {
            var cut = RenderComponent<MainLayout>();

            var main = cut.Find("main");
            Assert.NotNull(main);

            var article = cut.Find("article.content.px-4");
            Assert.NotNull(article);
        }


    }
}
