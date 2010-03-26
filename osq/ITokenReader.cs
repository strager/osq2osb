namespace osq {
    public interface ITokenReader {
        Location CurrentLocation {
            get;
        }

        Token ReadToken();
        Token PeekToken();
    }
}
