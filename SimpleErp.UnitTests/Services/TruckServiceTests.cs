using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using SimpleErp.Models;
using SimpleErp.Models.Contract;
using SimpleErp.Repository.TruckRepository;
using SimpleErp.Services;

namespace SimpleErp.UnitTests.Services;

[TestFixture]
public class TruckServiceTests
{
    private Mock<ITruckRepository> truckRepositoryMock;
    private Mock<IMapper> mapperMock;
    private Mock<ILogger<TruckService>> loggerMock;

    private ITruckService truckService;

    [SetUp]
    public void Setup()
    {
        truckRepositoryMock = new Mock<ITruckRepository>();
        mapperMock = new Mock<IMapper>();
        loggerMock = new Mock<ILogger<TruckService>>();

        truckService = new TruckService(truckRepositoryMock.Object, mapperMock.Object, loggerMock.Object);
    }

    [Test]
    public void CheckIfTruckWithCodeExists_Exists_ReturnsTrue()
    {
        // Arrange
        const string truckCode = "ABC123";
        truckRepositoryMock.Setup(repo => repo.CheckIfTruckWithCodeExists(truckCode)).Returns(true);

        // Act
        var result = truckService.CheckIfTruckWithCodeExists(truckCode);

        // Assert
        Assert.IsTrue(result);
        truckRepositoryMock.Verify(repo => repo.CheckIfTruckWithCodeExists(truckCode), Times.Once);
    }

    [Test]
    public void CheckIfTruckWithCodeExists_NotExists_ReturnsFalse()
    {
        // Arrange
        const string truckCode = "ABC123";
        truckRepositoryMock.Setup(repo => repo.CheckIfTruckWithCodeExists(truckCode)).Returns(false);

        // Act
        var result = truckService.CheckIfTruckWithCodeExists(truckCode);

        // Assert
        Assert.IsFalse(result);
        truckRepositoryMock.Verify(repo => repo.CheckIfTruckWithCodeExists(truckCode), Times.Once);
    }

    [Test]
    public void GetTrucksForUser_ReturnsMappedTrucks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var trucksInDb = new List<Truck> { new Truck { UserId = userId }, new Truck { UserId = userId } };
        var expectedTruckResponses = trucksInDb.Select(t => new TruckResponse()).ToList();

        truckRepositoryMock.Setup(repo => repo.Get(It.IsAny<Expression<Func<Truck, bool>>>()))
            .Returns(trucksInDb.AsQueryable());
        mapperMock.Setup(mapper => mapper.Map<IEnumerable<TruckResponse>>(trucksInDb))
            .Returns(expectedTruckResponses);

        // Act
        var result = truckService.GetTrucksForUser(userId);

        // Assert
        Assert.AreEqual(expectedTruckResponses, result);
        truckRepositoryMock.Verify(repo => repo.Get(It.IsAny<Expression<Func<Truck, bool>>>()), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<IEnumerable<TruckResponse>>(trucksInDb), Times.Once);
    }

    [Test]
    public async Task GetTruckByIdForUser_TruckExistsAndBelongsToUser_ReturnsMappedTruck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var truckInDb = new Truck { Id = truckId, UserId = userId };
        var expectedTruckResponse = new TruckResponse();

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync(truckInDb);
        mapperMock.Setup(mapper => mapper.Map<TruckResponse>(truckInDb))
            .Returns(expectedTruckResponse);

        // Act
        var result = await truckService.GetTruckByIdForUser(truckId, userId);

        // Assert
        Assert.AreEqual(expectedTruckResponse, result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(truckInDb), Times.Once);
    }

    [Test]
    public async Task GetTruckByIdForUser_TruckDoesNotBelongToUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var truckInDb = new Truck { Id = truckId, UserId = Guid.NewGuid() }; // Truck belongs to a different user

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync(truckInDb);

        // Act
        var result = await truckService.GetTruckByIdForUser(truckId, userId);

        // Assert
        Assert.IsNull(result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(It.IsAny<Truck>()), Times.Never);
    }

    [Test]
    public async Task GetTruckByIdForUser_TruckDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync((Truck)null);

        // Act
        var result = await truckService.GetTruckByIdForUser(truckId, userId);

        // Assert
        Assert.IsNull(result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(It.IsAny<Truck>()), Times.Never);
    }

    [Test]
    public async Task GetTruckByIdForUser_EmptyTruckId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await truckService.GetTruckByIdForUser(Guid.Empty, userId);

        // Assert
        Assert.IsNull(result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(It.IsAny<Truck>()), Times.Never);
    }

    [Test]
    public async Task GetFilteredTrucksForUser_ReturnsFilteredAndSortedTrucks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var filterOptions = new TruckFilterOptions
        {
            Status = "Loading",
            SearchTerm = "Truck",
            SortBy = "Name",
            Descending = false
        };
        var trucksInDb = new List<Truck>
        {
            new Truck { UserId = userId, Status = TruckStatus.Loading, Name = "Truck B" },
            new Truck { UserId = userId, Status = TruckStatus.Loading, Name = "Truck A" }
        };
        var expectedTruckResponses = trucksInDb.OrderBy(t => t.Name).Select(t => new TruckResponse()).ToList();

        truckRepositoryMock.Setup(repo => repo.Get(It.IsAny<Expression<Func<Truck, bool>>>()))
            .Returns(trucksInDb.AsQueryable());
        mapperMock.Setup(mapper => mapper.Map<IEnumerable<TruckResponse>>(It.IsAny<IEnumerable<Truck>>()))
            .Returns(expectedTruckResponses);

        // Act
        var result = truckService.GetFilteredTrucksForUser(filterOptions, userId);

        // Assert
        Assert.AreEqual(expectedTruckResponses, result);
        truckRepositoryMock.Verify(repo => repo.Get(It.IsAny<Expression<Func<Truck, bool>>>()), Times.Once);
    }

    [Test]
    public async Task AddTruckForUser_ValidTruckRequest_ReturnsMappedTruckResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckRequest = new TruckRequest { Code = "ABC123", Name = "Truck1", Status = TruckStatus.Loading };
        var truckToAdd = new Truck { UserId = userId, Code = "ABC123", Name = "Truck1", Status = TruckStatus.Loading };
        var addedTruckInDb = new Truck { Id = Guid.NewGuid(), UserId = userId, Code = "ABC123", Name = "Truck1", Status = TruckStatus.Loading };
        var expectedTruckResponse = new TruckResponse();

        truckRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Truck>())).ReturnsAsync(addedTruckInDb);
        mapperMock.Setup(mapper => mapper.Map<Truck>(truckRequest)).Returns(truckToAdd);
        mapperMock.Setup(mapper => mapper.Map<TruckResponse>(addedTruckInDb)).Returns(expectedTruckResponse);

        // Act
        var result = await truckService.AddTruckForUser(truckRequest, userId);

        // Assert
        Assert.AreEqual(expectedTruckResponse, result);
        truckRepositoryMock.Verify(repo => repo.AddAsync(truckToAdd), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(addedTruckInDb), Times.Once);
    }

    [Test]
    public async Task AddTruckForUser_DuplicateCode_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckRequest = new TruckRequest { Code = "ABC123", Name = "Truck1", Status = TruckStatus.Loading };

        truckRepositoryMock.Setup(repo => repo.CheckIfTruckWithCodeExists("ABC123")).Returns(true);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => truckService.AddTruckForUser(truckRequest, userId));
        truckRepositoryMock.Verify(repo => repo.CheckIfTruckWithCodeExists("ABC123"), Times.Once);
    }

    [Test]
    public async Task UpdateTruckForUser_ValidTruckRequest_ReturnsMappedTruckResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var truckRequest = new TruckRequest { Code = "ABC123", Name = "UpdatedTruck", Status = TruckStatus.AtJob };
        var existingTruckInDb = new Truck { Id = truckId, UserId = userId, Code = "ABC123", Name = "Truck1", Status = TruckStatus.Loading };
        var updatedTruckInDb = new Truck { Id = truckId, UserId = userId, Code = "ABC123", Name = "UpdatedTruck", Status = TruckStatus.AtJob };
        var expectedTruckResponse = new TruckResponse();

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync(existingTruckInDb);
        truckRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Truck>())).ReturnsAsync(updatedTruckInDb);
        mapperMock.Setup(mapper => mapper.Map<Truck>(truckRequest)).Returns(updatedTruckInDb);
        mapperMock.Setup(mapper => mapper.Map<TruckResponse>(updatedTruckInDb)).Returns(expectedTruckResponse);

        // Act
        var result = await truckService.UpdateTruckForUser(truckId, truckRequest, userId);

        // Assert
        Assert.AreEqual(expectedTruckResponse, result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        truckRepositoryMock.Verify(repo => repo.UpdateAsync(updatedTruckInDb), Times.Once);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(updatedTruckInDb), Times.Once);
    }

    [Test]
    public async Task UpdateTruckForUser_TruckDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync((Truck)null);

        // Act
        var result = await truckService.UpdateTruckForUser(truckId, new TruckRequest(), userId);

        // Assert
        Assert.IsNull(result);
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        truckRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Truck>()), Times.Never);
        mapperMock.Verify(mapper => mapper.Map<Truck>(It.IsAny<TruckRequest>()), Times.Never);
        mapperMock.Verify(mapper => mapper.Map<TruckResponse>(It.IsAny<Truck>()), Times.Never);
    }

    [Test]
    public async Task DeleteTruckForUser_TruckExistsAndBelongsToUser_DeletesTruck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var truckInDb = new Truck { Id = truckId, UserId = userId };

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync(truckInDb);

        // Act
        await truckService.DeleteTruckForUser(truckId, userId);

        // Assert
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        truckRepositoryMock.Verify(repo => repo.DeleteAsync(truckId), Times.Once);
    }

    [Test]
    public async Task DeleteTruckForUser_TruckDoesNotBelongToUser_DoesNotDeleteTruck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var truckId = Guid.NewGuid();
        var truckInDb = new Truck { Id = truckId, UserId = Guid.NewGuid() }; // Truck belongs to a different user

        truckRepositoryMock.Setup(repo => repo.GetByIdAsync(truckId)).ReturnsAsync(truckInDb);

        // Act
        await truckService.DeleteTruckForUser(truckId, userId);

        // Assert
        truckRepositoryMock.Verify(repo => repo.GetByIdAsync(truckId), Times.Once);
        truckRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
