namespace Cirreum.Contracts.Tests;

using Cirreum.Invocation;

public class HostInvocationFactoryExtensionsTests {

	private static readonly AsyncLocal<string?> Ambient = new();

	private static (IHostInvocationFactory Factory, IHostInvocation Invocation, IInvocationContext Context) CreateFactory() {
		var context = Substitute.For<IInvocationContext>();
		var invocation = Substitute.For<IHostInvocation>();
		invocation.Context.Returns(context);
		var factory = Substitute.For<IHostInvocationFactory>();
		factory.Begin(Arg.Any<HostInvocationOptions?>()).Returns(invocation);
		return (factory, invocation, context);
	}

	[Fact]
	public async Task StartDetached_RunsWorkInsideBegunInvocation() {
		var (factory, invocation, context) = CreateFactory();
		IInvocationContext? seen = null;

		await factory.StartDetached(ctx => {
			seen = ctx;
			return Task.CompletedTask;
		});

		seen.Should().BeSameAs(context);
		factory.Received(1).Begin(null);
		await invocation.Received(1).DisposeAsync();
	}

	[Fact]
	public async Task StartDetached_PassesOptionsThrough() {
		var (factory, _, _) = CreateFactory();
		var options = new HostInvocationOptions { Source = InvocationSources.Queue };

		await factory.StartDetached(_ => Task.CompletedTask, options);

		factory.Received(1).Begin(options);
	}

	[Fact]
	public async Task StartDetached_DoesNotFlowCallerExecutionContext() {
		var (factory, _, _) = CreateFactory();
		Ambient.Value = "caller";
		string? seen = "unset";

		await factory.StartDetached(_ => {
			seen = Ambient.Value;
			return Task.CompletedTask;
		});

		seen.Should().BeNull();
		Ambient.Value.Should().Be("caller");
	}

	[Fact]
	public async Task StartDetached_DisposesInvocation_WhenWorkThrows() {
		var (factory, invocation, _) = CreateFactory();

		var act = () => factory.StartDetached(_ => throw new InvalidOperationException("boom"));

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
		await invocation.Received(1).DisposeAsync();
	}

	[Fact]
	public async Task StartDetached_PropagatesBeginFailure() {
		var factory = Substitute.For<IHostInvocationFactory>();
		factory.Begin(Arg.Any<HostInvocationOptions?>())
			.Returns(_ => throw new InvalidOperationException("nested"));
		var ran = false;

		var act = () => factory.StartDetached(_ => { ran = true; return Task.CompletedTask; });

		await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("nested");
		ran.Should().BeFalse();
	}

	[Fact]
	public async Task StartDetached_ThrowsOnNullWork() {
		var (factory, _, _) = CreateFactory();

		var act = () => factory.StartDetached(null!);

		await act.Should().ThrowAsync<ArgumentNullException>();
	}

}
