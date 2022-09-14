using System;
using Unity.Mathematics;



namespace IzBone.Common {
static public partial class Math8 {

	/**
	 * 値を一定区間蓄積して、そこから平均を求めるためのモジュール
	 */
	public sealed class LinearSampler {

		public LinearSampler(int sampleNum = 5) {
			_buf = new float[sampleNum];
			clear();
		}

		/** 値を蓄積 */
		public void addSample(float val) {
			_buf[_curIdx] = val;
			_curIdx = (_curIdx+1) % _buf.Length;
			_sampleNum = math.min(_sampleNum+1, _buf.Length);
		}

		/** 蓄積している値をクリア */
		public void clear() {
			_curIdx = _sampleNum = 0;
		}

		/** 現在まで蓄積している情報から、平均を算出 */
		public float getAverage() {
			if (_sampleNum == 0) return 0;
			float ret = 0;
			for (int i=0; i<_sampleNum; ++i) ret += _buf[i];
			return ret / _sampleNum;
		}

		float[] _buf;
		int _curIdx, _sampleNum;
	}

	/**
	 * 値を一定区間蓄積して、そこから平均を求めるためのモジュール
	 */
	public sealed class LinearSamplerD {

		public LinearSamplerD(int sampleNum = 5) {
			_buf = new double[sampleNum];
			clear();
		}

		/** 値を蓄積 */
		public void addSample(double val) {
			_buf[_curIdx] = val;
			_curIdx = (_curIdx+1) % _buf.Length;
			_sampleNum = math.min(_sampleNum+1, _buf.Length);
		}

		/** 蓄積している値をクリア */
		public void clear() {
			_curIdx = _sampleNum = 0;
		}

		/** 現在まで蓄積している情報から、平均を算出 */
		public double getAverage() {
			if (_sampleNum == 0) return 0;
			double ret = 0;
			for (int i=0; i<_sampleNum; ++i) ret += _buf[i];
			return ret / _sampleNum;
		}

		double[] _buf;
		int _curIdx, _sampleNum;
	}


} }

