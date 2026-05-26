import Room from '../models/Room.js';
import { AppError } from '../utils/AppError.js';

// Create a new room
export const createRoom = async (data, user) => {
  const { name, type, visibility, password, maxPlayers } = data;

  if (visibility === 'private' && (!password || password.trim().length < 1)) {
    throw new AppError('Password is required for private rooms.', 400, 'VALIDATION_ERROR');
  }

  const room = await Room.create({
    name,
    type,
    visibility,
    password: visibility === 'private' ? password : undefined,
    maxPlayers: maxPlayers || 50,
    createdBy: user.id,
    creatorName: data.creatorName || 'Unknown',
    players: [{ userId: user.id, username: data.creatorName || 'Host' }],
  });

  return room;
};

// List active rooms (public + user's own private)
export const listRooms = async (userId) => {
  const rooms = await Room.find({
    status: 'active',
    $or: [
      { visibility: 'public' },
      { createdBy: userId },
      { 'players.userId': userId },
    ],
  }).sort({ createdAt: -1 });

  return rooms;
};

// Get a single room by ID
export const getRoom = async (roomId) => {
  const room = await Room.findById(roomId);
  if (!room) throw new AppError('Room not found.', 404, 'NOT_FOUND');
  return room;
};

// Update relay join code (called after Unity allocates relay)
export const setRelayCode = async (roomId, relayJoinCode, userId) => {
  const room = await Room.findById(roomId);
  if (!room) throw new AppError('Room not found.', 404, 'NOT_FOUND');
  if (room.createdBy.toString() !== userId) {
    throw new AppError('Only the room creator can set the relay code.', 403, 'ACCESS_DENIED');
  }
  room.relayJoinCode = relayJoinCode;
  await room.save();
  return room;
};

// Join a room (validates password for private rooms)
export const joinRoom = async (roomId, userId, username, password) => {
  const room = await Room.findById(roomId);
  if (!room) throw new AppError('Room not found.', 404, 'NOT_FOUND');
  if (room.status !== 'active') throw new AppError('Room is closed.', 400, 'ROOM_CLOSED');

  if (room.players.length >= room.maxPlayers) {
    throw new AppError('Room is full.', 400, 'ROOM_FULL');
  }

  // Check if already in room
  const alreadyIn = room.players.some(p => p.userId.toString() === userId);
  if (alreadyIn) return room;

  // Password check for private rooms
  if (room.visibility === 'private') {
    if (!password) throw new AppError('Password required for private rooms.', 401, 'PASSWORD_REQUIRED');
    const isMatch = await room.comparePassword(password);
    if (!isMatch) throw new AppError('Incorrect room password.', 401, 'PASSWORD_INVALID');
  }

  room.players.push({ userId, username: username || 'Player' });
  await room.save();
  return room;
};

// Leave a room
export const leaveRoom = async (roomId, userId) => {
  const room = await Room.findById(roomId);
  if (!room) throw new AppError('Room not found.', 404, 'NOT_FOUND');

  room.players = room.players.filter(p => p.userId.toString() !== userId);

  // Close room if empty
  if (room.players.length === 0) {
    room.status = 'closed';
    room.closedAt = new Date();
  }

  await room.save();
  return room;
};
