// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Security;

public interface ISecretProtector { string Protect(string plaintext, string purpose); string Unprotect(string protectedValue, string purpose); }
