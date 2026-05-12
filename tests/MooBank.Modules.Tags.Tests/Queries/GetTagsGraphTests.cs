#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

/// <summary>
/// Tests for the GetTagsGraph query handler.
/// Returns a flat {nodes, edges} graph for the user's family, including
/// every non-deleted tag and every parent-&gt;child edge between them.
/// </summary>
[Trait("Category", "Unit")]
public class GetTagsGraphTests
{
    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given no tags exist
    /// When the handler runs
    /// Then it returns an empty graph
    /// </summary>
    [Fact]
    public async Task Handle_EmptyTags_ReturnsEmptyGraph()
    {
        var tags = TestEntities.CreateTagQueryable([]);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Empty(result.Nodes);
        Assert.Empty(result.Edges);
    }

    /// <summary>
    /// Given a tag with no parents and no children (a true root with no descendants)
    /// When the handler runs
    /// Then it appears in Nodes with no Edges referring to it
    /// </summary>
    [Fact]
    public async Task Handle_StandaloneTag_IncludedAsNode_NoEdges()
    {
        var familyId = _mocks.User.FamilyId;
        var standalone = TestEntities.CreateTag(id: 1, name: "Standalone", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(standalone);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Single(result.Nodes);
        Assert.Equal("Standalone", result.Nodes[0].Name);
        Assert.Empty(result.Edges);
    }

    /// <summary>
    /// Given a parent with one child
    /// When the handler runs
    /// Then both nodes are returned and a single edge parent-&gt;child is returned
    /// </summary>
    [Fact]
    public async Task Handle_ParentChild_ReturnsBothNodes_AndEdge()
    {
        var familyId = _mocks.User.FamilyId;
        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var child = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        parent.Tags.Add(child);
        child.TaggedTo.Add(parent);

        var tags = TestEntities.CreateTagQueryable(parent, child);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Nodes.Count);
        Assert.Single(result.Edges);
        Assert.Equal(1, result.Edges[0].ParentId);
        Assert.Equal(2, result.Edges[0].ChildId);
    }

    /// <summary>
    /// Given a tag with two parents (multi-parent relationship)
    /// When the handler runs
    /// Then the node appears once and two edges are returned
    /// </summary>
    [Fact]
    public async Task Handle_MultiParent_ReturnsTwoEdges_OneNode()
    {
        var familyId = _mocks.User.FamilyId;
        var living = TestEntities.CreateTag(id: 1, name: "Living", familyId: familyId);
        var luxury = TestEntities.CreateTag(id: 2, name: "Luxury", familyId: familyId);
        var sport = TestEntities.CreateTag(id: 3, name: "Sport", familyId: familyId);

        living.Tags.Add(sport);
        luxury.Tags.Add(sport);
        sport.TaggedTo.Add(living);
        sport.TaggedTo.Add(luxury);

        var tags = TestEntities.CreateTagQueryable(living, luxury, sport);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(1, result.Nodes.Count(n => n.Id == 3));
        Assert.Equal(2, result.Edges.Count(e => e.ChildId == 3));
        Assert.Contains(result.Edges, e => e.ParentId == 1 && e.ChildId == 3);
        Assert.Contains(result.Edges, e => e.ParentId == 2 && e.ChildId == 3);
    }

    /// <summary>
    /// Given tags belonging to two families
    /// When the handler runs as a user from family A
    /// Then only family A tags and edges are returned
    /// </summary>
    [Fact]
    public async Task Handle_FiltersByUserFamily()
    {
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();

        var mine = TestEntities.CreateTag(id: 1, name: "Mine", familyId: userFamilyId);
        var theirs = TestEntities.CreateTag(id: 2, name: "Theirs", familyId: otherFamilyId);

        var tags = TestEntities.CreateTagQueryable(mine, theirs);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Single(result.Nodes);
        Assert.Equal("Mine", result.Nodes[0].Name);
    }

    /// <summary>
    /// Given a deleted tag
    /// When the handler runs
    /// Then the deleted node and any edges referencing it are excluded
    /// </summary>
    [Fact]
    public async Task Handle_ExcludesDeletedTagsAndTheirEdges()
    {
        var familyId = _mocks.User.FamilyId;
        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var live = TestEntities.CreateTag(id: 2, name: "Live", familyId: familyId);
        var dead = TestEntities.CreateTag(id: 3, name: "Dead", familyId: familyId, deleted: true);

        parent.Tags.Add(live);
        parent.Tags.Add(dead);
        live.TaggedTo.Add(parent);
        dead.TaggedTo.Add(parent);

        var tags = TestEntities.CreateTagQueryable(parent, live, dead);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Nodes.Count);
        Assert.DoesNotContain(result.Nodes, n => n.Name == "Dead");
        Assert.Single(result.Edges);
        Assert.Equal(2, result.Edges[0].ChildId);
    }

    /// <summary>
    /// Given a tag with colour and settings
    /// When the handler runs
    /// Then those values flow through to the TagNode unchanged
    /// </summary>
    [Fact]
    public async Task Handle_PreservesColourAndSettings()
    {
        var familyId = _mocks.User.FamilyId;
        var tag = TestEntities.CreateTag(
            id: 1,
            name: "Coloured",
            familyId: familyId,
            colour: new Asm.Drawing.HexColour("#aabbcc"),
            applySmoothing: true,
            excludeFromReporting: true);

        var tags = TestEntities.CreateTagQueryable(tag);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        var node = Assert.Single(result.Nodes);
        Assert.Equal("#aabbcc", node.Colour?.ToString(), StringComparer.OrdinalIgnoreCase);
        Assert.True(node.Settings.ApplySmoothing);
        Assert.True(node.Settings.ExcludeFromReporting);
    }
}
