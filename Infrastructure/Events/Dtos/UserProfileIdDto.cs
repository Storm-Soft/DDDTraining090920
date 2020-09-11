using System;

namespace DDDTraining.Tests
{
    public struct UserProfileIdDto
    {
        public Guid UserId { get; set; }

        public UserProfileId ToDomain()
            => new UserProfileId(UserId);
    }
}
