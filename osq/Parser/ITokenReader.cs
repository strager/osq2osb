namespace osq.Parser {
    public interface ITokenReader {
        Location CurrentLocation {
            get;
        }

        Token ReadToken();
    }
}


