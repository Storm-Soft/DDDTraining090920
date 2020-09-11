using System;

namespace DDDTraining.Tests
{
    public struct UserProfileId
    {
        public Guid UserId { get; set; }
        public UserProfileId(Guid userId)
        {
            UserId = userId;
        }
    }
}
