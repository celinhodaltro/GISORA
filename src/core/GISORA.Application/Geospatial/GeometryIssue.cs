using GISORA.Domain.Geospatial;

namespace GISORA.Application.Geospatial;

public sealed record GeometryIssue(string Code, string Message, Coordinate? Location = null);