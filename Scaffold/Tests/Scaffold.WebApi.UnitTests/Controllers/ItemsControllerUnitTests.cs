namespace Scaffold.WebApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Scaffold.Application.Exceptions;
    using Scaffold.Application.Features.Item;
    using Scaffold.WebApi.Controllers;
    using Scaffold.WebApi.Views;
    using Scaffold.WebApi.Views.MappingProfiles;
    using Xunit;

    public class ItemsControllerUnitTests
    {
        private readonly IMapper mapper;

        public ItemsControllerUnitTests()
        {
            ItemMappingProfile profile = new ItemMappingProfile();
            MapperConfiguration configuration = new MapperConfiguration(config => config.AddProfile(profile));
            this.mapper = configuration.CreateMapper();
        }

        public class Post : ItemsControllerUnitTests
        {
            [Fact]
            public async Task When_CreatingItem_Expect_CreatedAtRouteResult()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<AddItem.Command>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AddItem.Response { Item = new Domain.Aggregates.Bucket.Item() });

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                ActionResult result;

                // Act
                result = await controller.Post(new Random().Next(int.MaxValue), new Item());

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result);

                CreatedAtRouteResult actionResult = result as CreatedAtRouteResult;

                Assert.NotEmpty(actionResult.RouteName);
                Assert.IsType<Item>(actionResult.Value);
            }
        }

        public class Get : ItemsControllerUnitTests
        {
            [Fact]
            public async Task When_GettingItems_Expect_Items()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<GetItems.Query>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new GetItems.Response { Items = new List<Domain.Aggregates.Bucket.Item>() });

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                IList<Item> result;

                // Act
                result = await controller.Get(new Random().Next(int.MaxValue));

                // Assert
                Assert.NotNull(result);
            }

            [Fact]
            public async Task When_GettingItem_Expect_Item()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<GetItem.Query>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new GetItem.Response { Item = new Domain.Aggregates.Bucket.Item() });

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                Item result;

                // Act
                result = await controller.Get(new Random().Next(int.MaxValue), new Random().Next(int.MaxValue));

                // Assert
                Assert.NotNull(result);
            }

            [Fact]
            public async Task When_GettingNonExistingItem_Expect_ItemNotFoundException()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<GetItem.Query>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new GetItem.Response());

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                Exception exception;

                // Act
                exception = await Record.ExceptionAsync(() =>
                    controller.Get(new Random().Next(int.MaxValue), new Random().Next(int.MaxValue)));

                // Assert
                Assert.NotNull(exception);
                Assert.IsType<ItemNotFoundException>(exception);
            }
        }

        public class Put : ItemsControllerUnitTests
        {
            [Fact]
            public async Task When_UpdatingItem_Expect_UpdatedItem()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<UpdateItem.Command>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UpdateItem.Response
                    {
                        Item = new Domain.Aggregates.Bucket.Item(),
                        Created = false,
                    });

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                ActionResult<Item> result;

                // Act
                result = await controller.Put(new Random().Next(int.MaxValue), new Random().Next(int.MaxValue), new Item());

                // Assert
                Assert.Null(result.Result);
                Assert.IsType<Item>(result.Value);
            }

            [Fact]
            public async Task When_UpdatingNonExistingItem_Expect_CreatedAtRouteResult()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<UpdateItem.Command>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UpdateItem.Response
                    {
                        Item = new Domain.Aggregates.Bucket.Item(),
                        Created = true,
                    });

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                ActionResult<Item> result;

                // Act
                result = await controller.Put(new Random().Next(int.MaxValue), new Random().Next(int.MaxValue), new Item());

                // Assert
                Assert.IsType<CreatedAtRouteResult>(result.Result);
                Assert.Null(result.Value);

                CreatedAtRouteResult actionResult = result.Result as CreatedAtRouteResult;

                Assert.NotEmpty(actionResult.RouteName);
                Assert.IsType<Item>(actionResult.Value);
            }
        }

        public class Delete : ItemsControllerUnitTests
        {
            [Fact]
            public async Task When_DeletingItem_Expect_NoContentResult()
            {
                // Arrange
                Mock<IMediator> mock = new Mock<IMediator>();
                mock.Setup(m => m.Send(It.IsAny<RemoveItem.Command>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(default(Unit));

                ItemsController controller = new ItemsController(this.mapper, mock.Object);

                ActionResult result;

                // Act
                result = await controller.Delete(new Random().Next(int.MaxValue), new Random().Next(int.MaxValue));

                // Assert
                Assert.IsType<NoContentResult>(result);
            }
        }
    }
}