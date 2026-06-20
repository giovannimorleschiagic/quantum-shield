using QuantumShield.Be.Domain.Models;

namespace QuantumShield.Be.Domain.Interfaces;

public interface IEvaluationArtifactStore
{
    Task<string> SaveAsync(EvaluationArtifactDocument artifact, CancellationToken cancellationToken);

    Task<EvaluationArtifactDocument?> GetAsync(string blobName, CancellationToken cancellationToken);
}
