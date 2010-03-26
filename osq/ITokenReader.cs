namespace osq {
    public interface ITokenReader {
        Token ReadToken();
        Token PeekToken();
    }
}
