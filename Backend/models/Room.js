import mongoose from 'mongoose';
import bcrypt from 'bcrypt';

const roomSchema = new mongoose.Schema(
  {
    name: {
      type: String,
      required: [true, 'Room name is required'],
      trim: true,
      maxlength: [100, 'Room name cannot exceed 100 characters'],
    },
    type: {
      type: String,
      enum: {
        values: ['auditorium', 'classroom', 'big_auditorium'],
        message: 'Room type must be auditorium, classroom, or big_auditorium',
      },
      required: [true, 'Room type is required'],
    },
    visibility: {
      type: String,
      enum: {
        values: ['public', 'private'],
        message: 'Visibility must be public or private',
      },
      default: 'public',
    },
    password: {
      type: String,
      select: false,
    },
    relayJoinCode: {
      type: String,
      default: null,
    },
    createdBy: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'User',
      required: true,
    },
    creatorName: {
      type: String,
      trim: true,
    },
    players: [
      {
        userId: { type: mongoose.Schema.Types.ObjectId, ref: 'User' },
        username: { type: String, trim: true },
        joinedAt: { type: Date, default: Date.now },
      },
    ],
    maxPlayers: {
      type: Number,
      default: 50,
      min: 2,
      max: 100,
    },
    status: {
      type: String,
      enum: {
        values: ['active', 'closed'],
        message: 'Status must be active or closed',
      },
      default: 'active',
    },
    closedAt: {
      type: Date,
      default: null,
    },
  },
  {
    timestamps: true,
    versionKey: false,
    toJSON: {
      transform(doc, ret) {
        delete ret.password;
        ret.playerCount = ret.players ? ret.players.length : 0;
        return ret;
      },
    },
  }
);

// Hash password before saving (only for private rooms)
roomSchema.pre('save', async function (next) {
  if (!this.isModified('password') || !this.password) return next();
  this.password = await bcrypt.hash(this.password, 10);
  next();
});

// Compare password for private room joining
roomSchema.methods.comparePassword = async function (candidatePassword) {
  const room = await Room.findById(this._id).select('+password');
  if (!room.password) return true;
  return bcrypt.compare(candidatePassword, room.password);
};

const Room = mongoose.model('Room', roomSchema);

export default Room;
