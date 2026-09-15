using Sonban.Api.Classes;

namespace UnitTests.Mocks {
    internal class SettingsProviderMock : ISettingsProvider {
        public T GetValue<T>(string name) {
            switch (name) {
                case "BooksPath":
                case "DefaultBookCoverPath":
                case "BookConverterPath":
                    if (typeof(T) == typeof(string)) {
                        return (T)(object)"Default mock value";
                    }
                    break;
            }
            throw new NotImplementedException();
        }
    }
}
