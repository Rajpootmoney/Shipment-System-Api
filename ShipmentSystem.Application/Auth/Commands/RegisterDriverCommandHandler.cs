﻿using MediatR;
using ShipmentSystem.Application.Auth.Models;
using ShipmentSystem.Application.Exceptions;
using ShipmentSystem.Application.Interfaces;
using ShipmentSystem.Application.Interfaces.Auth;
using ShipmentSystem.Domain.Common;
using ShipmentSystem.Domain.Entities;

namespace ShipmentSystem.Application.Auth.Commands;

public class RegisterDriverCommandHandler : IRequestHandler<RegisterDriverCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IObjectMapper _mapper;

    public RegisterDriverCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IObjectMapper mapper
    )
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(
        RegisterDriverCommand request,
        CancellationToken cancellationToken
    )
    {
        var existing = await _unitOfWork.Users.FindByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ConflictException("Email already exists.");

        var driver = _mapper.Map<Driver>(request);

        await _unitOfWork.Users.AddAsync(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(_mapper.Map<JwtUserPayload>(driver));
        return Result<string>.Ok(token);
    }
}
