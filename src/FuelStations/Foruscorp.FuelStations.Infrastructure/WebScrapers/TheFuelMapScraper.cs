using Microsoft.Playwright;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;

namespace Foruscorp.FuelStations.Infrastructure.WebScrapers
{
    public class TheFuelMapScraper : ITheFuelMapScraper, IDisposable
    {
        private readonly IPlaywright _playwright;
        private readonly IBrowser _browser;
        private const string LoginUrl = "https://app.thefuelmap.com/login";
        private const string RouteApiUrl = "https://fuel.fulgertransport.com/api/web/get-route";
        private const int DefaultTimeoutMs = 30000;
        private const int ShortDelayMs = 500;
        private const int MediumDelayMs = 1000;
        private const int LongDelayMs = 5000;

        private const string defaultOrigin = "Los Angeles, Калифорния, США";
        private const string defaultDestination = "Los Angeles, Калифорния, США";

        private string bearerToken = string.Empty;

        public TheFuelMapScraper()
        {
            var _playwright = Playwright.CreateAsync().GetAwaiter().GetResult();
            _browser = _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            }).GetAwaiter().GetResult();
        }


        public async Task<string> ParseDataByUrlWithLogin()
        {
            var context = await CreateBrowserContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                await NavigateToLoginPageAsync(page);
                await PerformLoginAsync(page);
                await FillRouteFieldsAsync(page);

                await InterceptBearerTokenAsync(page);
                await SubmitRouteRequestAsync(page);

                return RemoveBearer(bearerToken); 
            }
            catch (Exception ex)
            {
                LogError(ex, await page.ContentAsync());
                throw;
            }
            finally
            {
                // Оставляем открытым для отладки, в продакшене можно закрыть
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }

        private async Task<IBrowserContext> CreateBrowserContextAsync()
        {
            return await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
                Locale = "en-US",
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                IgnoreHTTPSErrors = true
            });
        }

        private async Task NavigateToLoginPageAsync(IPage page)
        {
            Console.WriteLine("Navigating to login page...");
            await page.GotoAsync(LoginUrl, new PageGotoOptions { Timeout = DefaultTimeoutMs });

            await EnsurePageLoadedAsync(page, "Login page loaded successfully");
            await CheckAccessRestrictionAsync(page, "initial page load");
        }

        private async Task PerformLoginAsync(IPage page)
        {
            Console.WriteLine("Waiting for login form to be ready...");
            await WaitForLoginFormAsync(page);

            Console.WriteLine("Typing username...");
            await page.TypeAsync("input[name='username']", "Truckag");
            await page.Mouse.MoveAsync(300, 200);

            Console.WriteLine("Typing password...");
            await page.TypeAsync("input[name='password']", "ag3613");
            await page.Mouse.MoveAsync(300, 300);

            Console.WriteLine("Clicking Sign In button...");
            await page.ClickAsync("button[type='submit']");
            await Task.Delay(MediumDelayMs);

            Console.WriteLine("Waiting for login response...");
            await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DefaultTimeoutMs });
        }

        private async Task FillRouteFieldsAsync(IPage page, string origin = defaultOrigin, string destination = defaultOrigin)
        {
            Console.WriteLine("Waiting for Origin and Destination elements...");
            //await Task.Delay(ShortDelayMs);

            await EnsurePageLoadedAsync(page, "Origin and Destination elements loaded ");

            await WaitForRouteFieldsAsync(page);

            await FillAutocompleteFieldAsync(page, "input[placeholder='Origin']", defaultOrigin);
            await FillAutocompleteFieldAsync(page, "input[placeholder='Destination']", defaultOrigin);
        }

        private async Task<string> InterceptBearerTokenAsync(IPage page)
        {


            page.Request += async (sender, request) =>
            {
                if (request.Url == RouteApiUrl && request.Method == "POST")
                {
                    bearerToken = request.Headers["authorization"];
                    Console.WriteLine($"Intercepted Authorization: {bearerToken}");
                }
            };

            return bearerToken;
        }

        private async Task SubmitRouteRequestAsync(IPage page)
        {
            await Task.Delay(500);

            Console.WriteLine("Clicking Show route button...");
            await page.ClickAsync("button.chakra-button.css-shzi2m");
            await Task.Delay(LongDelayMs);

            Console.WriteLine("Route displayed. New page title: " + await page.TitleAsync());
        }

        // Вспомогательные методы
        private async Task EnsurePageLoadedAsync(IPage page, string successMessage)
        {
            await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = DefaultTimeoutMs });
            Console.WriteLine($"{successMessage}. Title: {await page.TitleAsync()}");
        }

        private async Task CheckAccessRestrictionAsync(IPage page, string context)
        {
            var content = await page.ContentAsync();
            if (content.Contains("Access restricted"))
            {
                throw new Exception($"Access restricted detected on {context}!");
            }
        }

        private async Task WaitForLoginFormAsync(IPage page)
        {
            await Task.WhenAll(
                page.WaitForSelectorAsync("input[name='username']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs }),
                page.WaitForSelectorAsync("input[name='password']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs })
            );
        }

        private async Task WaitForRouteFieldsAsync(IPage page)
        {
            await Task.WhenAll(
                page.WaitForSelectorAsync("input[placeholder='Origin']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs }),
                page.WaitForSelectorAsync("input[placeholder='Destination']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeoutMs })
            );
            Console.WriteLine("Both elements are visible!");
        }

        private async Task FillAutocompleteFieldAsync(IPage page, string selector, string value)
        {
            Console.WriteLine($"Filling field with selector '{selector}'...");
            await page.TypeAsync(selector, value, new PageTypeOptions { Delay = 100 });
            await Task.Delay(MediumDelayMs);

            Console.WriteLine("Selecting from autocomplete...");
            await page.PressAsync(selector, "ArrowDown");
            await Task.Delay(ShortDelayMs);
            await page.PressAsync(selector, "Enter");
        }

        private void LogError(Exception ex, string pageContent)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Current page content: {pageContent}");
        }


        private string RemoveBearer(string input)
        {
            if (input.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return input.Substring(7); 
            }
            return input;
        }

        public void Dispose()
        {
            _browser?.CloseAsync().GetAwaiter().GetResult();
            _playwright?.Dispose();
        }




        public async Task<string> GetBearerToken()
        {
            var context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
                Locale = "en-US",
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                IgnoreHTTPSErrors = true
            });
            var page = await context.NewPageAsync();

            try
            {
                Console.WriteLine("Navigating to login page...");
                await page.GotoAsync("https://app.thefuelmap.com/login", new PageGotoOptions { Timeout = 30000 });

                var initialContent = await page.ContentAsync();
                if (initialContent.Contains("Access restricted"))
                {
                    throw new Exception("Access restricted detected on initial page load!");
                }
                Console.WriteLine("Login page loaded successfully. Title: " + await page.TitleAsync());



                Console.WriteLine("Waiting for login form to be ready...");

                await Task.WhenAll(
                    page.WaitForSelectorAsync("input[name='username']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 30000 }),
                    page.WaitForSelectorAsync("input[name='password']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 30000 })
                    );

                Console.WriteLine("Login page loaded successfully. Title: " + await page.TitleAsync());



                //await Task.Delay(500);
                Console.WriteLine("Typing username...");
                await page.TypeAsync("input[name='username']", "Truckag", new PageTypeOptions { Delay = 100 });

                await page.Mouse.MoveAsync(300, 300);
                await Task.Delay(500);
                Console.WriteLine("Typing password...");
                await page.TypeAsync("input[name='password']", "ag3613", new PageTypeOptions { Delay = 100 });

                await Task.Delay(1000);
                Console.WriteLine("Clicking Sign In button...");
                await page.ClickAsync("button[type='submit']");

                // Ждем загрузки страницы после логина
                Console.WriteLine("Waiting for login response...");
                await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = 30000 });

                Console.WriteLine("Waiting for Origin and Destination elements...");

                await Task.WhenAll(
                    page.WaitForSelectorAsync("input[placeholder='Origin']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 30000 }),
                    page.WaitForSelectorAsync("input[placeholder='Destination']", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 30000 })
                );

                Console.WriteLine("Both elements are visible!");

                //await Task.Delay(5000);

                // Работа с полем Origin (с автодополнением)
                Console.WriteLine("Filling Origin field...");
                var originInput = "input[placeholder='Origin']";
                await page.TypeAsync(originInput, "Los Angeles, Калифорния, США", new PageTypeOptions { Delay = 100 });

                await Task.Delay(1000);

                // Ждем появления выпадающего списка (pac-item - стандартный класс для Google Places)
                Console.WriteLine("Waiting for autocomplete suggestions...");
                //await page.WaitForSelectorAsync(".pac-item", new PageWaitForSelectorOptions { Timeout = 5000 });

                // Выбираем первый элемент из списка (нажимаем ArrowDown и Enter)
                await page.PressAsync(originInput, "ArrowDown");
                await Task.Delay(500); // Небольшая задержка для стабильности
                await page.PressAsync(originInput, "Enter");

                // Работа с полем Destination (с автодополнением)


                Console.WriteLine("Filling Destination field...");
                var destinationInput = "input[placeholder='Destination']";
                await page.TypeAsync(destinationInput, "Las Vegas, Невада, США", new PageTypeOptions { Delay = 100 });

                await Task.Delay(1000);

                // Ждем появления выпадающего списка
                Console.WriteLine("Waiting for autocomplete suggestions...");
                //await page.WaitForSelectorAsync(".pac-item", new PageWaitForSelectorOptions { Timeout = 5000 });

                // Выбираем первый элемент из списка
                await page.PressAsync(destinationInput, "ArrowDown");
                await Task.Delay(500);
                await page.PressAsync(destinationInput, "Enter");

                await Task.Delay(500);

                string bearerToken = "";

                page.Request += async (sender, request) =>
                {
                    if (request.Url == "https://fuel.fulgertransport.com/api/web/get-route" && request.Method == "POST")
                    {
                        bearerToken = request.Headers["authorization"];
                        Console.WriteLine($"Intercepted Authorization: {bearerToken}");
                    }
                };

                // Нажимаем кнопку "Show route"
                Console.WriteLine("Clicking Show route button...");
                await page.ClickAsync("button.chakra-button.css-shzi2m");

                await Task.Delay(5000);

                // Ждем загрузки маршрута (можно добавить ожидание какого-то элемента, связанного с маршрутом)
                //await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions { Timeout = 30000 });
                Console.WriteLine("Route displayed. New page title: " + await page.TitleAsync());

                RemoveBearer(bearerToken);

                return bearerToken;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Current page content: " + await page.ContentAsync());
                throw;
            }
            finally
            {
                // Оставляем браузер открытым для проверки
                await page.CloseAsync();
                await context.CloseAsync();
            }
        }

    
    }
}
